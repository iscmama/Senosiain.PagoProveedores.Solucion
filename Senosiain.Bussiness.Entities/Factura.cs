using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senosiain.Bussiness.Entities
{
    [Serializable]
    public partial class Factura
    {
        public Factura() { }

        public long NUMERO { set; get; }
        public DateTime FECHA { set; get; }
        public int IDPROVEEDOR { set; get; }
        public string NOMBREPROVEEDOR { set; get; }
        public int IDFACTURA { set; get; }
        public string CLAVEFACTURA { set; get; }
        public string REFERENCIA { set; get; }
        public decimal IMPORTE { set; get; }
        public string MONEDA { set; get; }
    }
}