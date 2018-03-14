namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class SetSceneDescriptorEventArgs : CommandRequestEventArgs
    {
        public bool HasValues { get; set; }

        public float FloorClipPlaneX { get; set; }
        public float FloorClipPlaneY { get; set; }
        public float FloorClipPlaneZ { get; set; }
        public float FloorClipPlaneW { get; set; }

        public float StageAreaCenterX { get; set; }
        public float StageAreaCenterY { get; set; }
        public float StageAreaCenterZ { get; set; }
        public float StageAreaSizeX { get; set; }
        public float StageAreaSizeY { get; set; }
        public float StageAreaSizeZ { get; set; }

        public float GameAreaCenterX { get; set; }
        public float GameAreaCenterY { get; set; }
        public float GameAreaCenterZ { get; set; }

        public float GameAreaSizeX { get; set; }
        public float GameAreaSizeY { get; set; }
        public float GameAreaSizeZ { get; set; }

        public float GameAreaInnerLimitsX { get; set; }
        public float GameAreaInnerLimitsY { get; set; }
        public float GameAreaInnerLimitsZ { get; set; }
    }
}
