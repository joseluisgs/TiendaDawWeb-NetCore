using NUnit.Framework;

// Desactivar el paralelismo a nivel de ensamblado para evitar bloqueos en SQLite In-Memory
[assembly: LevelOfParallelism(1)]
