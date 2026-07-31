using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wezit
{

    [Serializable]
    public class DBVersion
    {
        public string value;

        public DateTime Date
        {
            get => DateTime.Parse(this.value);
        }

        public override string ToString()
        {
            return String.Format(
                "Last update: {0}\n",
                value
            );
        }
    }
}

