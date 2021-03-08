using UnityEngine;
namespace LitEngine.Value
{
    public class TransformFixed
    {
        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }
        public TransformFixed(Transform trans)
        {
            SetTransform(trans);
        }

        public void SetTransform(Transform trans)
        {
            transform = trans;
            gameObject = trans.gameObject;
        }

        VectorFixed3 _position = new VectorFixed3();
        public VectorFixed3 position
        {
            get
            {
                _position.SetValue(transform.position);
                return _position;
            }
            set
            {
                _position = value;
                transform.position = _position.ToVector3();
            }
        }

        VectorFixed3 _localPosition = new VectorFixed3();
        public VectorFixed3 localPosition
        {
            get
            {
                _localPosition.SetValue(transform.localPosition);
                return _localPosition;
            }
            set
            {
                _localPosition = value;
                transform.localPosition = _localPosition.ToVector3();
            }
        }

        VectorFixed3 _forward = new VectorFixed3();
        public VectorFixed3 forward
        {
            get
            {
                _forward.SetValue(transform.forward);
                return _forward;
            }
        }

        VectorFixed3 _right = new VectorFixed3();
        public VectorFixed3 right
        {
            get
            {
                _right.SetValue(transform.right);
                return _right;
            }
        }

        VectorFixed4 _rotation = new VectorFixed4();
        public VectorFixed4 rotation
        {
            get
            {
                _rotation.SetValue(transform.rotation);
                return _rotation;
            }
            set
            {
                _rotation = value;
                transform.rotation = _rotation.ToQuaternion();
            }
        }

        VectorFixed4 _localRotation = new VectorFixed4();
        public VectorFixed4 localRotation
        {
            get
            {
                _localRotation.SetValue(transform.localRotation);
                return _localRotation;
            }
            set
            {
                _localRotation = value;
                transform.localRotation = _localRotation.ToQuaternion();
            }
        }

         
    }
}
