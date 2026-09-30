using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace RobloxServer.Data
{
    /// <summary>
    /// A tiny "table" persisted as XML in App_Data. Good enough for a private server and it runs
    /// the same on IIS (Windows) and Mono/XSP (Raspberry Pi) without a database engine.
    /// </summary>
    public class XmlTable<T> where T : class
    {
        readonly object sync = new object();
        readonly string path;
        List<T> rows;

        readonly string fileName;

        public XmlTable(string fileName)
        {
            this.fileName = fileName;
            path = Path.Combine(Config.DataPath, fileName);
        }

        List<T> Rows
        {
            get
            {
                if (rows == null)
                {
                    rows = Load();
                }
                return rows;
            }
        }

        List<T> Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            RemoteBlobStore.Pull(fileName, path);

            if (!File.Exists(path))
            {
                return new List<T>();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(List<T>));
                using (var stream = File.OpenRead(path))
                {
                    return (List<T>)serializer.Deserialize(stream) ?? new List<T>();
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "Could not read " + path + ": " + ex.Message);
                throw;
            }
        }

        void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            var serializer = new XmlSerializer(typeof(List<T>));
            using (var stream = File.Create(temp))
            {
                serializer.Serialize(stream, rows);
            }
            File.Copy(temp, path, true);
            File.Delete(temp);
            RemoteBlobStore.Push(fileName, path);
        }

        public List<T> All()
        {
            lock (sync)
            {
                return Rows.ToList();
            }
        }

        public T Find(Func<T, bool> predicate)
        {
            lock (sync)
            {
                return Rows.FirstOrDefault(predicate);
            }
        }

        public List<T> Where(Func<T, bool> predicate)
        {
            lock (sync)
            {
                return Rows.Where(predicate).ToList();
            }
        }

        public void Insert(T row)
        {
            lock (sync)
            {
                Rows.Add(row);
                Save();
            }
        }

        /// <summary>Runs <paramref name="change"/> on the first matching row and saves. Returns false if nothing matched.</summary>
        public bool Update(Func<T, bool> predicate, Action<T> change)
        {
            lock (sync)
            {
                T row = Rows.FirstOrDefault(predicate);
                if (row == null)
                {
                    return false;
                }
                change(row);
                Save();
                return true;
            }
        }

        public int Delete(Func<T, bool> predicate)
        {
            lock (sync)
            {
                int removed = Rows.RemoveAll(r => predicate(r));
                if (removed > 0)
                {
                    Save();
                }
                return removed;
            }
        }

        /// <summary>Inserts a row built from the next free id, atomically.</summary>
        public T InsertWithId(Func<T, long> getId, long firstId, Func<long, T> build)
        {
            lock (sync)
            {
                long next = Rows.Count == 0 ? firstId : Math.Max(firstId, Rows.Max(getId) + 1);
                T row = build(next);
                Rows.Add(row);
                Save();
                return row;
            }
        }
    }
}
