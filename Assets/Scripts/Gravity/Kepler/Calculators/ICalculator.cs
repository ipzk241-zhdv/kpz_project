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

/// <summary>Генерація точок орбіти (еліпс/гіпербола).</summary>
public interface IOrbitPointsGenerator
{
    void Generate(OrbitData target, ref Vector3d[] points, int count, Vector3d origin, double maxDistance);
}

/// <summary>Обчислення середньої аномалії з поточної позиції.</summary>
public interface IMeanAnomalyCalculator
{
    double Calculate(OrbitData target);
}