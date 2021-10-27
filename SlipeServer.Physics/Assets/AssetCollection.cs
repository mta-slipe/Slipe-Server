using RenderWareIo.Structs.Col;
using RenderWareIo.Structs.Dff;
using System.Collections.Generic;

namespace SlipeServer.Physics.Assets
{
    public class AssetCollection
    {
        private Dictionary<int, Dff> dffs;
        private Dictionary<int, Col> cols;

        public AssetCollection()
        {
            this.dffs = new();
            this.cols = new();
        }

        public Dff? GetDff(int model)
        {
            this.dffs.TryGetValue(model, out var dff);
            return dff;
        }

        public Col? GetCol(int model)
        {
            this.cols.TryGetValue(model, out var col);
            return col;
        }

        public void RegisterDff(int model, Dff dff)
        {
            this.dffs[model] = dff;
        }

        public void RegisterCol(int model, Col col)
        {
            this.cols[model] = col;
        }
    }
}
