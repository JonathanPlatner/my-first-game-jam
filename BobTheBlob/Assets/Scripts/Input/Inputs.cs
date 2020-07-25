using UnityEngine;

public class Inputs
{
    [System.Serializable]
    public class Button
    {
        [SerializeField]
        private KeyCode[] inputs;

        private bool value;
        private bool lastValue;
        public Button(KeyCode k)
        {
            inputs = new KeyCode[] { k };
        }
        public Button(KeyCode[] k)
        {
            inputs = k;
        }

        public bool Active()
        {
            foreach(KeyCode k in inputs)
            {
                if(k != KeyCode.None)
                {
                    if(Input.GetKey(k)) return true;
                }
            }
            return false;
        }

        public bool Down()
        {
            foreach(KeyCode k in inputs)
            {
                if(k != KeyCode.None)
                {
                    if(Input.GetKey(k))
                    {
                        value = true;
                        if(!lastValue)
                        {
                            lastValue = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public bool Up()
        {
            foreach(KeyCode k in inputs)
            {
                if(k != KeyCode.None)
                {
                    if(Input.GetKey(k))
                    {
                        return false;
                    }
                }
            }
            value = false;
            if(lastValue)
            {
                lastValue = false;
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    private struct AxisInput
    {
        [SerializeField]
        private KeyCode positive;
        [SerializeField]
        private KeyCode negative;

        public KeyCode Positive { get { return positive; } }
        public KeyCode Negative { get { return negative; } }
        public AxisInput(KeyCode p, KeyCode n)
        {
            positive = p;
            negative = n;
        }
    }
    [System.Serializable]
    public class Axis
    {
        [SerializeField]
        private AxisInput[] inputs;

        public Axis(KeyCode p, KeyCode n)
        {
            inputs = new AxisInput[] { new AxisInput(p, n) };
        }
        public Axis(KeyCode[] p, KeyCode[] n)
        {
            inputs = new AxisInput[p.Length];
            for(int i = 0; i < p.Length; i++)
            {
                inputs[i] = new AxisInput(p[i], n[i]);
            }
        }

        public float Value()
        {
            float value = 0;

            foreach(AxisInput i in inputs)
            {
                if(Input.GetKey(i.Positive))
                {
                    value += 1;
                    break;
                }
            }

            foreach(AxisInput i in inputs)
            {
                if(Input.GetKey(i.Negative))
                {
                    value -= 1;
                    break;
                }
            }
            return value;
        }

    }

    public class Mouse
    {
        public Vector2 Pixel()
        {
            return Input.mousePosition;
        }

        public Vector2 World(Camera c)
        {
            return c.ScreenToWorldPoint(Input.mousePosition);
        }

        public Ray Project(Camera c)
        {
            return c.ScreenPointToRay(Input.mousePosition);
        }
    }



    public static KeyCode GetNextKey()
    {
        foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if(Input.GetKey(key))
            {
                Debug.Log(key);
            }
        }
        return KeyCode.None;
    }
}
