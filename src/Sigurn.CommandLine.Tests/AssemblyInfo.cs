using Microsoft.VisualStudio.TestTools.UnitTesting;

// Включить параллельное выполнение тестов на уровне классов
// Workers = 0 означает использовать все доступные ядра процессора
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]