/// <summary>Базовий інтерфейс для обчислення стану орбіти за елементами.</summary>
public interface IOrbitElementsCalculator
{
    void CalculateOrbitStateFromOrbitalElements(OrbitData target);
}

/// <summary>Базовий інтерфейс для обчислення стану орбіти за векторами.</summary>
public interface IOrbitVectorsCalculator
{
    void CalculateOrbitStateFromOrbitalVectors(OrbitData target);
}