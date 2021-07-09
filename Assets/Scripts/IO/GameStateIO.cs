namespace CenturionRex.IO
{
    public static class GameStateIO
    {
        private class Packer : Sacristan.Ahhnold.IO.Serialized.SaveFile.Packer
        {
            protected override string Salt => "Y7fA7nEjt0Jl30M";
            protected override string FileName => "gs";
            protected override string Extension => ".centurion.rex";
            public override SerializationType SaveFileSerializationType { get; set; } = SerializationType.JSON;
        }

        public class Processor : Sacristan.Ahhnold.IO.Serialized.SaveFile.Processor
        {
            Packer _packer;

            public override Sacristan.Ahhnold.IO.Serialized.SaveFile.Packer SaveFilePacker
            {
                get
                {
                    if (_packer == null) _packer = new Packer();
                    return _packer;
                }
            }
        }

        public static Processor SaveGameProcessor = new Processor();
    }

}
