using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.Files {
    public abstract class BinHandler {
        protected Hashtable htDecryptBuffer;
        protected List<byte> lstbOutputBuffer;

        protected BinHandler() {
            htDecryptBuffer = new Hashtable();
            lstbOutputBuffer = new List<byte>();
            InitializeHexTable();
        }

        private void InitializeHexTable() {
            htDecryptBuffer.Clear();

            for (int i = 0; i <= 255; i++) {
                htDecryptBuffer.Add(i.ToString("X2").PadLeft(2, '0'), Convert.ToByte(i ^ 215));
            }
        }
    }
}
