using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IQuestionService
{
    List<Question> GetAllQuestions();
    Question? GetQuestionById(int id);
    Question GetRandomQuestion(List<int> excludeIds);
}

public class QuestionService : IQuestionService
{
    private readonly List<Question> _questions;

    public QuestionService()
    {
        _questions = InitializeQuestions();
    }

    public List<Question> GetAllQuestions()
    {
        return _questions;
    }

    public Question? GetQuestionById(int id)
    {
        return _questions.FirstOrDefault(q => q.Id == id);
    }

    public Question GetRandomQuestion(List<int> excludeIds)
    {
        var availableQuestions = _questions.Where(q => !excludeIds.Contains(q.Id)).ToList();
        
        if (availableQuestions.Count == 0)
        {
            return _questions[0]; // Return first question if all have been asked
        }

        var random = new Random();
        var index = random.Next(availableQuestions.Count);
        return availableQuestions[index];
    }

    private List<Question> InitializeQuestions()
    {
        return new List<Question>
        {
            // C# Basics - Multiple Choice
            new Question
            {
                Id = 1,
                Category = "C# Basics",
                Type = QuestionType.MultipleChoice,
                Text = "Quel mot-clé est utilisé pour définir une classe en C# ?",
                Choices = new List<string> { "class", "Class", "define", "type" },
                CorrectAnswer = "class",
                Explanation = "Le mot-clé 'class' est utilisé pour définir une nouvelle classe en C#."
            },
            new Question
            {
                Id = 2,
                Category = "C# Basics",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle est la valeur par défaut d'une variable de type 'int' non initialisée ?",
                Choices = new List<string> { "0", "null", "1", "undefined" },
                CorrectAnswer = "0",
                Explanation = "Les types valeur comme int ont une valeur par défaut de 0."
            },
            new Question
            {
                Id = 3,
                Category = "C# Basics",
                Type = QuestionType.MultipleChoice,
                Text = "Quel modificateur d'accès rend un membre accessible uniquement dans la classe elle-même ?",
                Choices = new List<string> { "private", "public", "protected", "internal" },
                CorrectAnswer = "private",
                Explanation = "Le modificateur 'private' restreint l'accès aux membres de la classe elle-même."
            },
            new Question
            {
                Id = 4,
                Category = "C# Basics",
                Type = QuestionType.FreeText,
                Text = "Combien de bits un type 'int' occupe-t-il en mémoire ?",
                Choices = new List<string>(),
                CorrectAnswer = "32",
                Explanation = "Un int en C# est un entier signé de 32 bits."
            },
            
            // OOP - Multiple Choice
            new Question
            {
                Id = 5,
                Category = "POO",
                Type = QuestionType.MultipleChoice,
                Text = "Quel pilier de la POO permet de cacher les détails d'implémentation ?",
                Choices = new List<string> { "Encapsulation", "Héritage", "Polymorphisme", "Abstraction" },
                CorrectAnswer = "Encapsulation",
                Explanation = "L'encapsulation permet de cacher les détails d'implémentation et de contrôler l'accès aux données."
            },
            new Question
            {
                Id = 6,
                Category = "POO",
                Type = QuestionType.MultipleChoice,
                Text = "Quel mot-clé permet à une classe d'hériter d'une autre classe ?",
                Choices = new List<string> { ":", "extends", "inherits", "implements" },
                CorrectAnswer = ":",
                Explanation = "En C#, on utilise ':' pour indiquer l'héritage, contrairement à 'extends' en Java."
            },
            new Question
            {
                Id = 7,
                Category = "POO",
                Type = QuestionType.MultipleChoice,
                Text = "Une classe abstraite peut-elle être instanciée ?",
                Choices = new List<string> { "Non", "Oui", "Seulement avec new", "Dépend du contexte" },
                CorrectAnswer = "Non",
                Explanation = "Une classe abstraite ne peut pas être instanciée directement, elle doit être héritée."
            },
            new Question
            {
                Id = 8,
                Category = "POO",
                Type = QuestionType.FreeText,
                Text = "Combien de classes une classe C# peut-elle hériter directement ?",
                Choices = new List<string>(),
                CorrectAnswer = "1",
                Explanation = "C# ne supporte que l'héritage simple, une classe ne peut hériter que d'une seule classe."
            },

            // Collections & LINQ
            new Question
            {
                Id = 9,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle collection garantit l'unicité des éléments ?",
                Choices = new List<string> { "HashSet", "List", "ArrayList", "Queue" },
                CorrectAnswer = "HashSet",
                Explanation = "HashSet garantit que tous les éléments sont uniques."
            },
            new Question
            {
                Id = 10,
                Category = "LINQ",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle méthode LINQ retourne le premier élément ou la valeur par défaut ?",
                Choices = new List<string> { "FirstOrDefault", "First", "Single", "Take" },
                CorrectAnswer = "FirstOrDefault",
                Explanation = "FirstOrDefault retourne le premier élément ou default(T) si la séquence est vide."
            },
            new Question
            {
                Id = 11,
                Category = "LINQ",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle méthode LINQ projette chaque élément d'une séquence ?",
                Choices = new List<string> { "Select", "Where", "OrderBy", "GroupBy" },
                CorrectAnswer = "Select",
                Explanation = "Select est utilisé pour projeter/transformer chaque élément d'une séquence."
            },
            new Question
            {
                Id = 12,
                Category = "Collections",
                Type = QuestionType.FreeText,
                Text = "Quelle est la complexité temporelle moyenne d'une recherche dans un Dictionary ?",
                Choices = new List<string>(),
                CorrectAnswer = "O(1)",
                Explanation = "Dictionary utilise une table de hachage, offrant une complexité O(1) en moyenne."
            },

            // Async/Await
            new Question
            {
                Id = 13,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "Quel mot-clé marque une méthode comme asynchrone ?",
                Choices = new List<string> { "async", "await", "Task", "asynchronous" },
                CorrectAnswer = "async",
                Explanation = "Le mot-clé 'async' permet de marquer une méthode comme asynchrone."
            },
            new Question
            {
                Id = 14,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "Quel type de retour une méthode async doit-elle avoir ?",
                Choices = new List<string> { "Task ou Task<T>", "void", "async", "Promise" },
                CorrectAnswer = "Task ou Task<T>",
                Explanation = "Les méthodes async retournent Task, Task<T>, ou void (déconseillé sauf pour les événements)."
            },
            new Question
            {
                Id = 15,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "Que fait le mot-clé 'await' ?",
                Choices = new List<string> { "Suspend l'exécution jusqu'à la fin de la tâche", "Crée un nouveau thread", "Bloque le thread", "Annule la tâche" },
                CorrectAnswer = "Suspend l'exécution jusqu'à la fin de la tâche",
                Explanation = "await suspend l'exécution de la méthode sans bloquer le thread."
            },

            // Exception Handling
            new Question
            {
                Id = 16,
                Category = "Exceptions",
                Type = QuestionType.MultipleChoice,
                Text = "Quel bloc est toujours exécuté, qu'une exception soit levée ou non ?",
                Choices = new List<string> { "finally", "catch", "try", "throw" },
                CorrectAnswer = "finally",
                Explanation = "Le bloc finally est toujours exécuté, même si une exception est levée."
            },
            new Question
            {
                Id = 17,
                Category = "Exceptions",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle est la classe de base de toutes les exceptions en .NET ?",
                Choices = new List<string> { "Exception", "Error", "SystemException", "BaseException" },
                CorrectAnswer = "Exception",
                Explanation = "System.Exception est la classe de base de toutes les exceptions en .NET."
            },

            // Delegates & Events
            new Question
            {
                Id = 18,
                Category = "Delegates",
                Type = QuestionType.MultipleChoice,
                Text = "Qu'est-ce qu'un delegate en C# ?",
                Choices = new List<string> { "Un type qui référence des méthodes", "Une classe", "Une interface", "Un type valeur" },
                CorrectAnswer = "Un type qui référence des méthodes",
                Explanation = "Un delegate est un type qui peut référencer des méthodes avec une signature spécifique."
            },
            new Question
            {
                Id = 19,
                Category = "Events",
                Type = QuestionType.MultipleChoice,
                Text = "Quel mot-clé est utilisé pour déclarer un événement ?",
                Choices = new List<string> { "event", "delegate", "action", "trigger" },
                CorrectAnswer = "event",
                Explanation = "Le mot-clé 'event' est utilisé pour déclarer un événement basé sur un delegate."
            },

            // Generics
            new Question
            {
                Id = 20,
                Category = "Génériques",
                Type = QuestionType.MultipleChoice,
                Text = "Quel symbole est utilisé pour déclarer un type générique ?",
                Choices = new List<string> { "<T>", "[T]", "(T)", "{T}" },
                CorrectAnswer = "<T>",
                Explanation = "Les chevrons <> sont utilisés pour déclarer et utiliser des types génériques."
            },
            new Question
            {
                Id = 21,
                Category = "Génériques",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle contrainte de type générique spécifie que T doit être un type référence ?",
                Choices = new List<string> { "where T : class", "where T : struct", "where T : new()", "where T : object" },
                CorrectAnswer = "where T : class",
                Explanation = "'where T : class' contraint T à être un type référence."
            },

            // Design Patterns
            new Question
            {
                Id = 22,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "Quel pattern garantit qu'une classe n'a qu'une seule instance ?",
                Choices = new List<string> { "Singleton", "Factory", "Observer", "Strategy" },
                CorrectAnswer = "Singleton",
                Explanation = "Le pattern Singleton garantit qu'une classe n'a qu'une seule instance globale."
            },
            new Question
            {
                Id = 23,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "Quel pattern permet de créer des objets sans spécifier leur classe exacte ?",
                Choices = new List<string> { "Factory", "Singleton", "Adapter", "Decorator" },
                CorrectAnswer = "Factory",
                Explanation = "Le pattern Factory permet de créer des objets sans exposer la logique de création."
            },

            // .NET Framework
            new Question
            {
                Id = 24,
                Category = ".NET",
                Type = QuestionType.MultipleChoice,
                Text = "Qu'est-ce que le CLR ?",
                Choices = new List<string> { "Common Language Runtime", "Common Library Repository", "Code Language Runtime", "Class Library Reference" },
                CorrectAnswer = "Common Language Runtime",
                Explanation = "CLR (Common Language Runtime) est l'environnement d'exécution de .NET."
            },
            new Question
            {
                Id = 25,
                Category = ".NET",
                Type = QuestionType.MultipleChoice,
                Text = "Quel est le nom du garbage collector de .NET ?",
                Choices = new List<string> { "GC", "MemoryManager", "Collector", "CleanUp" },
                CorrectAnswer = "GC",
                Explanation = "GC (Garbage Collector) gère automatiquement la mémoire en .NET."
            },
            new Question
            {
                Id = 26,
                Category = ".NET",
                Type = QuestionType.FreeText,
                Text = "Combien de générations le garbage collector .NET utilise-t-il ?",
                Choices = new List<string>(),
                CorrectAnswer = "3",
                Explanation = "Le GC .NET utilise 3 générations : 0, 1 et 2."
            },

            // String & Performance
            new Question
            {
                Id = 27,
                Category = "Performance",
                Type = QuestionType.MultipleChoice,
                Text = "Quelle classe est plus efficace pour concaténer de nombreuses chaînes ?",
                Choices = new List<string> { "StringBuilder", "String", "StringBuffer", "Concat" },
                CorrectAnswer = "StringBuilder",
                Explanation = "StringBuilder est optimisé pour les opérations de concaténation multiples."
            },
            new Question
            {
                Id = 28,
                Category = "String",
                Type = QuestionType.MultipleChoice,
                Text = "Les strings en C# sont-ils mutables ou immutables ?",
                Choices = new List<string> { "Immutables", "Mutables", "Dépend du contexte", "Les deux" },
                CorrectAnswer = "Immutables",
                Explanation = "Les strings en C# sont immutables, chaque modification crée une nouvelle instance."
            },

            // Value Types vs Reference Types
            new Question
            {
                Id = 29,
                Category = "Types",
                Type = QuestionType.MultipleChoice,
                Text = "Où sont stockés les types valeur ?",
                Choices = new List<string> { "Stack", "Heap", "Cache", "Registry" },
                CorrectAnswer = "Stack",
                Explanation = "Les types valeur sont généralement stockés sur la pile (stack)."
            },
            new Question
            {
                Id = 30,
                Category = "Types",
                Type = QuestionType.MultipleChoice,
                Text = "Quel mot-clé permet de définir un type valeur personnalisé ?",
                Choices = new List<string> { "struct", "class", "value", "type" },
                CorrectAnswer = "struct",
                Explanation = "Le mot-clé 'struct' permet de définir un type valeur personnalisé."
            },

            // Nullable Types
            new Question
            {
                Id = 31,
                Category = "Nullable",
                Type = QuestionType.MultipleChoice,
                Text = "Comment déclare-t-on un int nullable ?",
                Choices = new List<string> { "int?", "Nullable<int>", "int!", "nullint" },
                CorrectAnswer = "int?",
                Explanation = "'int?' est un raccourci pour 'Nullable<int>'."
            },
            new Question
            {
                Id = 32,
                Category = "Nullable",
                Type = QuestionType.MultipleChoice,
                Text = "Quel opérateur permet de fournir une valeur par défaut si null ?",
                Choices = new List<string> { "??", "?.", "!!", "??" },
                CorrectAnswer = "??",
                Explanation = "L'opérateur '??' (null-coalescing) fournit une valeur par défaut si l'opérande est null."
            },

            // Interface & Abstract
            new Question
            {
                Id = 33,
                Category = "Interface",
                Type = QuestionType.MultipleChoice,
                Text = "Une interface peut-elle contenir des champs ?",
                Choices = new List<string> { "Non", "Oui", "Seulement des constantes", "Seulement readonly" },
                CorrectAnswer = "Non",
                Explanation = "Une interface ne peut pas contenir de champs, seulement des propriétés, méthodes, etc."
            },
            new Question
            {
                Id = 34,
                Category = "Interface",
                Type = QuestionType.FreeText,
                Text = "Combien d'interfaces une classe peut-elle implémenter ?",
                Choices = new List<string>(),
                CorrectAnswer = "illimité",
                Explanation = "Une classe peut implémenter un nombre illimité d'interfaces."
            },

            // Access Modifiers
            new Question
            {
                Id = 35,
                Category = "Modificateurs",
                Type = QuestionType.MultipleChoice,
                Text = "Quel modificateur rend un membre accessible dans l'assembly ?",
                Choices = new List<string> { "internal", "public", "protected", "private" },
                CorrectAnswer = "internal",
                Explanation = "Le modificateur 'internal' limite l'accès à l'assembly courante."
            }
        };
    }
}
