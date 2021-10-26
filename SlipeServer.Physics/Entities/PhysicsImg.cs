using RenderWareIo;

namespace SlipeServer.Physics.Entities
{
    public class PhysicsImg
    {
        internal ImgFile imgFile;

        public PhysicsImg(string path)
        {
            this.imgFile = new ImgFile(path);
        }
    }
}
