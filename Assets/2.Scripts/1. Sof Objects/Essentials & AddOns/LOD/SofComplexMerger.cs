using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectLOD))]
public class SofComplexMerger
{
    public class MergedObject
    {
        public List<FilterRenderer> filtRends;
        public MeshRenderer renderer;
        private int lastCombineLength;
        public MergedObject(SofComponent parent, Material commonMaterial, string name)
        {
            filtRends = new List<FilterRenderer>();
            renderer = parent.transform.CreateChild(name).gameObject.AddComponent<MeshRenderer>();
            renderer.material = commonMaterial;
            renderer.gameObject.AddComponent<MeshFilter>();
            renderer.gameObject.layer = 9;
            parent.complex.lod.renderers.Add(renderer);
            lastCombineLength = 0;
        }

        public void CombineAndMerge()
        {
            if (lastCombineLength == filtRends.Count) return;
            

            CombineInstance[] combine = new CombineInstance[filtRends.Count];

            for (int i = 0; i < combine.Length; i++)
            {
                Transform root = renderer.transform.root;
                MeshFilter filter = filtRends[i].filter;
                combine[i].mesh = filter.sharedMesh;
                if (!filtRends[i].rend.gameObject.activeSelf) combine[i].mesh = new Mesh();

                Vector3 translate = root.InverseTransformPoint(filter.transform.position);
                Quaternion rotate = Quaternion.Inverse(root.rotation) * filter.transform.rotation;
                combine[i].transform = Matrix4x4.TRS(translate, rotate, Vector3.one);
            }

            MeshFilter mergeFilter = renderer.GetComponent<MeshFilter>();
            if (mergeFilter.sharedMesh == null) mergeFilter.sharedMesh = new Mesh();
            mergeFilter.sharedMesh.CombineMeshes(combine);

            lastCombineLength = filtRends.Count;
        }
    }
    public MergedObject fixedMerged;
    public MergedObject fullMerged;
    public List<FilterRenderer> mobileFiltRends;
    public List<FilterRenderer> unMergedFiltRends;


    public SofComplexMerger(SofComponent combineParent, Transform[] mobileFiltersExceptions)
    {
        List<FilterRenderer> relevantFiltRends = GetRelevantRenderers(combineParent);
        SplitRenderers(combineParent, relevantFiltRends, mobileFiltersExceptions);

        fixedMerged.CombineAndMerge();
        fullMerged.CombineAndMerge();

        foreach (FilterRenderer filter in fixedMerged.filtRends)
            filter.rend.enabled = false;
    }
    public void OnPartDetached(Renderer[] detachedRenderers)
    {
        foreach (Renderer renderer in detachedRenderers)
        {
            FilterRenderer filtRend = new FilterRenderer(renderer);
            if (unMergedFiltRends.Remove(filtRend)) continue;
            fullMerged.filtRends.Remove(filtRend);
            if (mobileFiltRends.Remove(filtRend)) continue;

            fixedMerged.filtRends.Remove(filtRend);
            renderer.enabled = true;
        }
        fixedMerged.CombineAndMerge();
        fullMerged.CombineAndMerge();
    }
    public void UpdateMergedModel()
    {
        fixedMerged.CombineAndMerge();
        fullMerged.CombineAndMerge();
    }

    private bool IsRelevant(Renderer renderer, Transform combineParent, LODGroup[] lodGroups)
    {
        foreach (LODGroup group in lodGroups)
        {
            if (combineParent.IsChildOf(group.transform)) continue;
            LOD[] lods = group.GetLODs();
            foreach (LOD subLod in lods)
                foreach (Renderer lodRend in subLod.renderers)
                    if (lodRend == renderer) return false;
        }
        return !renderer.GetComponent<ParticleSystem>() && !renderer.transform.IsChildOf(combineParent);
    }
    private List<FilterRenderer> GetRelevantRenderers(SofComponent combineParent)
    {
        List<FilterRenderer> relevantFiltRends = new List<FilterRenderer>();
        LODGroup[] lodGroups = combineParent.sofObject.GetComponentsInChildren<LODGroup>();

        foreach (Renderer rend in combineParent.complex.lod.renderers)
            if (IsRelevant(rend, combineParent.tr, lodGroups))
                relevantFiltRends.Add(new FilterRenderer(rend));

        return relevantFiltRends;
    }
    private void SplitRenderers(SofComponent combineParent, List<FilterRenderer> relevantFiltRends, Transform[] mobileFiltersExceptions)
    {
        HydraulicSystem[] hydraulics = combineParent.sofObject.GetComponentsInChildren<HydraulicSystem>();
        Material commonMaterial = combineParent.aircraft.GetComponentInChildren<FuselageCore>().GetComponent<Renderer>().sharedMaterial;

        unMergedFiltRends = new List<FilterRenderer>();
        mobileFiltRends = new List<FilterRenderer>();
        fixedMerged = new MergedObject(combineParent, commonMaterial, "Fixed Merged");
        fullMerged = new MergedObject(combineParent, commonMaterial, "Full Merged");

        foreach (FilterRenderer filtRend in relevantFiltRends)
        {
            if (!filtRend.IsMergeable(commonMaterial))
                unMergedFiltRends.Add(filtRend);
            else
            {   
                if (filtRend.IsMobile(hydraulics, mobileFiltersExceptions))
                    mobileFiltRends.Add(filtRend);
                else
                    fixedMerged.filtRends.Add(filtRend);
            }
        }
        fullMerged.filtRends.AddRange(mobileFiltRends);
        fullMerged.filtRends.AddRange(fixedMerged.filtRends);
    }
}