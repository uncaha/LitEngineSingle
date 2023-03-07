using System.Collections.Generic;
namespace LitEngine.Data
{
    public class DataAttribute
    {
        private Dictionary<string, DataField> attributes = null;
        public object this[string keyParameter]
        {
            get
            {
                if (attributes == null || !attributes.ContainsKey(keyParameter)) return null;
                return attributes[keyParameter].Value;
            }
            set
            {
                if (attributes == null) attributes = new Dictionary<string, DataField>();
                if (attributes.ContainsKey(keyParameter))
                {
                    if (value == null)
                        attributes.Remove(keyParameter);
                    else
                        attributes[keyParameter].Value = value;
                }
                else
                {
                    if (value != null)
                        attributes.Add(keyParameter, new DataField(keyParameter, value));
                }
            }
        }

        public void Load(IO.AESReader _loader)
        {
            int tattcount = _loader.ReadInt32();
            if (tattcount > 0)
            {
                attributes = new Dictionary<string, DataField>();
                for (int i = 0; i < tattcount; i++)
                {
                    DataField tfield = new DataField(null, null);
                    tfield.Load(_loader);
                    attributes.Add(tfield.Key, tfield);
                }
            }
        }

        public void Save(IO.AESWriter _writer)
        {
            int tattcount = attributes == null ? 0 : attributes.Count;
            _writer.WriteInt(tattcount);
            if (tattcount > 0)
            {
                foreach (KeyValuePair<string, DataField> pair in attributes)
                {
                    pair.Value.Save(_writer);
                }
            }
        }
    }
}
