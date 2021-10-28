using RenderWareIo.Structs.Col;
using RenderWareIo.Structs.Dff;
using System.Collections.Generic;

namespace SlipeServer.Physics.Assets
{
    public class AssetCollection
    {
        private readonly Dictionary<int, Dff> dffs;
        private readonly Dictionary<int, ColCombo> cols;

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

        public ColCombo? GetCol(int model)
        {
            this.cols.TryGetValue(model, out var colCombo);
            return colCombo;
        }

        public void RegisterDff(int model, Dff dff)
        {
            this.dffs[model] = dff;
        }

        public void RegisterCol(int model, ColCombo colCombo)
        {
            this.cols[model] = colCombo;
        }
    }
}
