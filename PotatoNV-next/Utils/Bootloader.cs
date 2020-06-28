using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PotatoNV_next.Utils
{
    public class Bootloader
    {
        public class Image
        {
            private bool? valid = null;

            public string Path { get; }
            public string Role { get; }
            public uint Address { get; }
            private string Hash { get; }
            public bool IsValid { get => valid ?? Validate(); }

            private bool Validate()
            {
                if (Hash == null)
                {
                    return true;
                }

                using (var stream = File.OpenRead(Path)) {
                    using (var sha1 = SHA1.Create())
                    {
                        stream.Position = 0;
                        byte[] hash = sha1.ComputeHash(stream);
                        stream.Close();
                        valid = BitConverter.ToString(hash).Replace("-", "").ToLower() == Hash;
                    }
                }

                return valid.Value;
            }

            public Image(string path, string role, uint address, string hash = null)
            {
                Path = path;
                Role = role;
                Address = address;
                Hash = hash;
            }
        }

        public Image[] Images { get; }
        public string Title { get; }
        public string Name { get; }

        public Bootloader(string name, string title, Image[] images)
        {
            Title = title;
            Name = name;
            Images = images;
        }

        private static uint ParseAddress(string str) => Convert.ToUInt32(str, str.StartsWith("0x") ? 16 : 10);

        private static XmlElement GetRootFromFile(string filename)
        {
            var xml = new XmlDocument();
            xml.Load(filename);

            var root = xml.DocumentElement;

            if (root.Name != "bootloader")
            {
                throw new Exception("XML root name is invalid.");
            }

            return root;
        }

        private readonly static string[] requiredStrings = new[] { "path", "role", "hash", "address" };

        public static Bootloader ParseBootloader(string filename)
        {
            var root = GetRootFromFile(filename);
            var dir = Path.GetDirectoryName(filename);

            var title = root.GetAttribute("name");

            if (string.IsNullOrEmpty(title))
            {
                Log.Info("Name attribute is invalid!");
                title = "Unknown bootloader";
            }

            var images = new List<Image>();

            foreach (XmlNode node in root)
            {
                bool isBad = node.Name != "image";

                foreach (var key in requiredStrings)
                {
                    var item = node.Attributes.GetNamedItem(key);
                    isBad |= item == null || string.IsNullOrWhiteSpace(item.Value);
                }

                if (isBad)
                {
                    throw new Exception("Failed to parse image");
                }

                images.Add(new Image(
                        Path.Combine(dir, node.Attributes.GetNamedItem("path").Value),
                        node.Attributes.GetNamedItem("role").Value,
                        ParseAddress(node.Attributes.GetNamedItem("address").Value),
                        node.Attributes.GetNamedItem("hash").Value
                    ));
            }

            return new Bootloader(Path.GetFileName(dir), title, images.ToArray());
        }
    }
}
