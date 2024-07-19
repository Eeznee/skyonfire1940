using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelManager
{   
    public List<Pack> packs;
    private int currentPack;

    public float fullThrottleCons;

    private SofComplex complex;

    public float totalCapacity { get; private set; }
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
    public bool Empty { get { return currentPack < 0; } }
    public float FuelTimer { get { return TotalFuel / fullThrottleCons * 3600f; } }

    
    public FuelManager(SofComplex _complex)
    {
        complex = _complex;
        totalCapacity = 0f;
        List<LiquidTank> fuelTanks = new List<LiquidTank>();
        foreach (LiquidTank liquidTank in complex.GetComponentsInChildren<LiquidTank>())
        {
            if (liquidTank.liquid && liquidTank.liquid.type == LiquidType.Fuel)
            {
                fuelTanks.Add(liquidTank);
                totalCapacity += liquidTank.capacity;
            }
        }

        fuelTanks.Sort(SortByPosition);

        packs = new List<Pack>();

        while (fuelTanks.Count > 0)
        {
            Pack newPack = new Pack(fuelTanks[0], fuelTanks);
            packs.Add(newPack);
            fuelTanks.Remove(newPack.mainTank);
            fuelTanks.Remove(newPack.symmetricTank);
        }
        currentPack = packs.Count - 1;

        if (complex.aircraft)
        {
            EnginePreset engine = complex.aircraft.engines.preset;
            float c = engine.ConsumptionRate(1f, engine.gear1.Evaluate(2000f) * 745.7f);
            fullThrottleCons = c * complex.aircraft.engines.all.Length;
        }
    }

    const float a = 1f / 3600f;
    public void Consume(float kgPerHour,float deltaTime) { Consume(kgPerHour * deltaTime * a); }
    public void Consume(float amount)
    {
        packs[currentPack].Consume(amount);
        if (packs[currentPack].fluidMass <= 0f) currentPack--;
    }

    static int SortByPosition(LiquidTank f1, LiquidTank f2)
    {
        if (Mathf.Abs(f1.localPos.x) > Mathf.Abs(f2.localPos.x) + 1f) return 1;
        else if (Mathf.Abs(f1.localPos.x) < Mathf.Abs(f2.localPos.x) - 1f) return -1;

        float f1DistanceCog = Mathf.Abs(f1.complex.cogForwardDistance - f1.localPos.z);
        float f2DistanceCog = Mathf.Abs(f1.complex.cogForwardDistance - f2.localPos.z);
        return f1DistanceCog > f2DistanceCog ? 1 : -1;
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
            consumedMass *= multiplier * -1f;
            mainTank.ShiftFluidMass(consumedMass);
            symmetricTank?.ShiftFluidMass(consumedMass);
        }
        static bool IsSymmetrical(LiquidTank f1, LiquidTank f2)
        {
            if (f1 == f2) return false;

            Vector3 f1LocalPos = f1.complex.tr.InverseTransformPoint(f1.tr.position);
            if (Mathf.Abs(f1LocalPos.x) < 0.2f) return false;

            Vector3 f2LocalPos = f2.complex.tr.InverseTransformPoint(f2.tr.position);
            f2LocalPos.x *= -1f;

            return (f1LocalPos - f2LocalPos).magnitude < 1f;
        }
    }
}
