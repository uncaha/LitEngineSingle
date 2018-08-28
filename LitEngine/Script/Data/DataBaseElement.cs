
namespace LitEngine.Data
{
    public abstract class DataBaseElement
    {
        public abstract void Load(LitEngine.IO.AESReader _loader);
        public abstract void Save(LitEngine.IO.AESWriter _writer);
        public DataAttribute Attribut { get; private set; }

        public DataBaseElement()
        {
            Attribut = new DataAttribute();
        }

        public T TryGetAttribute<T>(string keyParameter, object _defaultValue = null)
        {
            try
            {
                object obj = Attribut[keyParameter];
                checked
                {
                    return obj != null ? (T)obj : _defaultValue == null ? default(T) : (T)_defaultValue;
                }
            }
            catch (System.Exception erro)
            {
                DLog.LogError(erro.ToString());
            }
            return _defaultValue == null ? default(T) : (T)_defaultValue;
        }
    }

}
