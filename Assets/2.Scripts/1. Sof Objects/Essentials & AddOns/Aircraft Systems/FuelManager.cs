using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FuelManager
{   
    public List<Pack> packs;
    public float fullThrottleCons;


    private SofModular modular;
    private List<LiquidTank> fuelTanks;
    private int currentPack;

    public bool Empty { get; private set; }
    public float TotalCapacity { get; private set; }
    public float TotalFuel
    {
        get
        {
            float total = 0f;
            for (int i = 0; i <= currentPack; i++)
                total += packs[i].fluidMass;
            return total;
        }
    }
    public float FuelTimer { get { return TotalFuel / fullThrottleCons * 3600f; } }

    
    public FuelManager(SofModular _complex)
    {
        modular = _complex;
        TotalCapacity = 0f;
        fuelTanks = new List<LiquidTank>();
        foreach (LiquidTank liquidTank in modular.GetComponentsInChildren<LiquidTank>())
        {
            if (liquidTank.liquid && liquidTank.liquid.type == LiquidType.Fuel)
            {
                fuelTanks.Add(liquidTank);
                TotalCapacity += liquidTank.capacity;
            }
        }

        fuelTanks.Sort(SortByPosition);

        modular.OnInitialize += OnInitialize;
    }

    private void OnInitialize()
    {
        packs = new List<Pack>();

        while (fuelTanks.Count > 0)
        {
            Pack newPack = new Pack(fuelTanks[0], fuelTanks);
            packs.Add(newPack);
            fuelTanks.Remove(newPack.mainTank);
            fuelTanks.Remove(newPack.symmetricTank);
        }
        currentPack = packs.Count - 1;
        if(currentPack < 0)
            Empty = true;
        else
            Consume(0f);
        

        float totalConsumption = 0f;

        if (!modular.aircraft) return;

        foreach (Engine engine in modular.aircraft.engines.AllEngines)
        {
            if (engine.Preset == null) continue;

            if (engine.Class == EngineClass.JetEngine)
            {
                JetEngine jetEngine = engine as JetEngine;

                totalConsumption += jetEngine.JetPreset.MaxThrust * jetEngine.JetPreset.FuelConsumption(1f);
            }
            if (engine.Class == EngineClass.PistonEngine)
            {
                PistonEngine pistonEngine = engine as PistonEngine;

                totalConsumption += pistonEngine.PistonPreset.HighestContinuousPower * pistonEngine.PistonPreset.FuelConsumption(1f);
            }
        }
        fullThrottleCons = totalConsumption;
    }

    const float a = 1f / 3600f;
    public void Consume(float kgPerHour,float deltaTime) { Consume(kgPerHour * deltaTime * a); }
    public void Consume(float amount)
    {
        packs[currentPack].Consume(amount);
        if (packs[currentPack].fluidMass <= 0f)
        {
            currentPack--;
            if (currentPack < 0) Empty = true;
        }
    }

    static int SortByPosition(LiquidTank f1, LiquidTank f2)
    {
        if (Mathf.Abs(f1.localPos.x) > Mathf.Abs(f2.localPos.x) + 1f) return 1;
        else if (Mathf.Abs(f1.localPos.x) < Mathf.Abs(f2.localPos.x) - 1f) return -1;

        if(f1.sofComplex && f2.sofComplex)
        {
            float f1DistanceCog = Mathf.Abs(f1.sofComplex.cogForwardDistance - f1.localPos.z);
            float f2DistanceCog = Mathf.Abs(f2.sofComplex.cogForwardDistance - f2.localPos.z);
            return f1DistanceCog > f2DistanceCog ? 1 : -1;
        }
        else
        {
            float f1DistanceCog = Mathf.Abs(f1.localPos.z);
            float f2DistanceCog = Mathf.Abs(f2.localPos.z);
            return f1DistanceCog > f2DistanceCog ? 1 : -1;
        }
    }


    public class Pack
    {
        public LiquidTank mainTank;
        public LiquidTank symmetricTank;

        public float fluidMass { get { return mainTank.fluidMass + (symmetricTank ? symmetricTank.fluidMass : 0f); } }

        public Pack(LiquidTank _mainTank, List<LiquidTank> allTanks)
        {
            mainTank = _mainTank;

            foreach (LiquidTank tryFuelTank in allTanks)
                if (IsSymmetrical(mainTank, tryFuelTank))
                    symmetricTank = tryFuelTank;
        }
        public void Consume(float consumedMass)
        {
            float multiplier = 0.5f;
            if (mainTank.Empty) multiplier *= 2f;
            if (!symmetricTank || symmetricTank.Empty) multiplier *= 2f;

            consumedMass *= multiplier;

            mainTank.ShiftFluidMass(-consumedMass);
            if(symmetricTank) symmetricTank.ShiftFluidMass(-consumedMass);
        }
        static bool IsSymmetrical(LiquidTank f1, LiquidTank f2)
        {
            if (f1 == f2) return false;

            Vector3 f1LocalPos = f1.sofModular.tr.InverseTransformPoint(f1.tr.position);
            if (Mathf.Abs(f1LocalPos.x) < 0.2f) return false;

            Vector3 f2LocalPos = f2.sofModular.tr.InverseTransformPoint(f2.tr.position);
            f2LocalPos.x *= -1f;

            return (f1LocalPos - f2LocalPos).magnitude < 1f;
        }
    }
}
