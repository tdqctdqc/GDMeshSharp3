
    using Godot;

    public class Hill : Landform, IDecaledTerrain
    {
        public Hill() : 
            base("Hill", .4f, .5f, Colors.Brown, false, .2f)
        {
        }

        Mesh IDecaledTerrain.GetDecal()
        {
            var spacing = ((IDecaledTerrain) this).DecalSpacing;
            var offset = Vector2.Down * spacing / 2f;
            return MeshGenerator.GetArrayMesh(new Vector2[]{Vector2.Left * spacing + offset,
                Vector2.Right * spacing + offset,
                Vector2.Up * spacing + offset});
        }

        float IDecaledTerrain.DecalSpacing => 20f;
    }