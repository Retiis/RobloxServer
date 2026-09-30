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
    ///
    /// IMPORTANT: every read re-loads from disk (and, on Vercel, re-pulls from the remote blob
    /// first -- see RemoteBlobStore). We used to cache the deserialized rows in memory for the
    /// lifetime of the process, but on Vercel more than one container instance can be alive at
    /// once (or an old one can linger after a redeploy); a cached copy in one instance would never
    /// see rows written by another, which looked like random logouts / "missing" accounts. Reading
    /// fresh every time costs a small XML parse, which is fine for a private server's traffic.
    /// </summary>
    public class XmlTable<T> where T : class
    {
        readonly object sync = new object();
        readonly string path;
        readonly string fileName;

        public XmlTable(string fileName)
        {
            this.fileName = fileName;
            path = Path.Combine(Config.DataPath, fileName);
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

        void Save(List<T> rows)
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
                return Load();
            }
        }

        public T Find(Func<T, bool> predicate)
        {
            lock (sync)
            {
                return Load().FirstOrDefault(predicate);
            }
        }

        public List<T> Where(Func<T, bool> predicate)
        {
            lock (sync)
            {
                return Load().Where(predicate).ToList();
            }
        }

        public void Insert(T row)
        {
            lock (sync)
            {
                List<T> rows = Load();
                rows.Add(row);
                Save(rows);
            }
        }

        /// <summary>Runs <paramref name="change"/> on the first matching row and saves. Returns false if nothing matched.</summary>
        public bool Update(Func<T, bool> predicate, Action<T> change)
        {
            lock (sync)
            {
                List<T> rows = Load();
                T row = rows.FirstOrDefault(predicate);
                if (row == null)
                {
                    return false;
                }
                change(row);
                Save(rows);
                return true;
            }
        }

        public int Delete(Func<T, bool> predicate)
        {
            lock (sync)
            {
                List<T> rows = Load();
                int removed = rows.RemoveAll(r => predicate(r));
                if (removed > 0)
                {
                    Save(rows);
                }
                return removed;
            }
        }

        /// <summary>Inserts a row built from the next free id, atomically.</summary>
        public T InsertWithId(Func<T, long> getId, long firstId, Func<long, T> build)
        {
            lock (sync)
            {
                List<T> rows = Load();
                long next = rows.Count == 0 ? firstId : Math.Max(firstId, rows.Max(getId) + 1);
                T row = build(next);
                rows.Add(row);
                Save(rows);
                return row;
            }
        }
    }
}
