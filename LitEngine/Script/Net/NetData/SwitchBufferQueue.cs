using System;
using System.Runtime.InteropServices;
namespace LitEngine.Net
{
    public class BufferObjectTag
    {
        public int startIndex;
        public int length;
    }

    public class SwitchBufferObject
    {
        byte[] _bufferBytes = new byte[1024 * 100];
        int endIndex;

        public byte[] bufferBytes { get { return _bufferBytes; } }
        public void Clear()
        {
            endIndex = 0;
        }

        public int PushBytes(byte[] pBytes, int pStartIndex, int pLen)
        {
            if(endIndex + pLen >= _bufferBytes.Length)
            {
                int tlen = _bufferBytes.Length * 2 + pLen;
                byte[] temp = new byte[tlen];
                Buffer.BlockCopy(_bufferBytes, 0, temp, 0, endIndex);
                _bufferBytes = temp;
            }
            int retStart = endIndex;

            Buffer.BlockCopy(pBytes, pStartIndex, _bufferBytes, retStart, pLen);
            endIndex += pLen;

            return retStart;
        }

        public byte[] PopBytes(BufferObjectTag pTag)
        {
            if (pTag == null) return null;
            if(endIndex + pTag.length >= _bufferBytes.Length)
            {
                DLog.LogError("PopBytes: There is not enough length.");
                return null;
            }
            byte[] ret = new byte[pTag.length];
            Buffer.BlockCopy(_bufferBytes, pTag.startIndex, ret, 0, pTag.length);
            return ret;
        }
    }

    public class SwitchBufferQueue
    {
        SwitchBufferObject pushBufferObject = new SwitchBufferObject();
        SwitchBufferObject popBufferObject = new SwitchBufferObject();

        public SwitchBufferQueue()
        {
        }

        void Swap()
        {
            SwitchBufferObject temp = pushBufferObject;
            pushBufferObject = popBufferObject;
            popBufferObject = temp;

            pushBufferObject.Clear();
        }

        public void Switch()
        {
            lock (pushBufferObject)
            {
                Swap();
            }
        }

        public void Clear()
        {
            lock (pushBufferObject)
            {
                pushBufferObject.Clear();
                popBufferObject.Clear();
            }
        }

        public int PushBytes(byte[] pBytes,int pStartIndex,int pLen)
        {
            return pushBufferObject.PushBytes(pBytes, pStartIndex, pLen);
        }

        public byte[] PopBytes(BufferObjectTag pTag)
        {
            return popBufferObject.PopBytes(pTag);
        }

        public byte[] PopBuffer
        {
            get
            {
                return popBufferObject.bufferBytes;
            }
        }
    }
}
