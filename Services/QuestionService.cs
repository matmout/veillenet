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
    private List<Question> _shuffledQuestions;

    public QuestionService()
    {
        _questions = InitializeQuestions();
        _shuffledQuestions = Shuffle(_questions);
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
        excludeIds ??= new List<int>();

        // Choose uniformly at random among non-excluded questions
        var candidates = _questions.Where(q => !excludeIds.Contains(q.Id)).ToList();
        if (candidates.Count > 0)
        {
            var idx = Random.Shared.Next(candidates.Count);
            return candidates[idx];
        }

        // All questions are excluded; reshuffle and return the first
        _shuffledQuestions = Shuffle(_questions);
        return _shuffledQuestions[0];
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
                Text = "Which keyword is used to define a class in C#?",
                Choices = new List<string> { "class", "Class", "define", "type" },
                CorrectAnswer = "class",
                Explanation = "The 'class' keyword is used to define a new class in C#."
            },
            new Question
            {
                Id = 2,
                Category = "C# Basics",
                Type = QuestionType.MultipleChoice,
                Text = "What is the default value of an uninitialized 'int' variable?",
                Choices = new List<string> { "0", "null", "1", "undefined" },
                CorrectAnswer = "0",
                Explanation = "Value types like int have a default value of 0."
            },
            new Question
            {
                Id = 3,
                Category = "C# Basics",
                Type = QuestionType.MultipleChoice,
                Text = "Which access modifier makes a member accessible only within the class itself?",
                Choices = new List<string> { "private", "public", "protected", "internal" },
                CorrectAnswer = "private",
                Explanation = "The 'private' modifier restricts access to members of the class itself."
            },
            new Question
            {
                Id = 4,
                Category = "C# Basics",
                Type = QuestionType.FreeText,
                Text = "How many bits does an 'int' type occupy in memory?",
                Choices = new List<string>(),
                CorrectAnswer = "32",
                Explanation = "An int in C# is a signed 32-bit integer."
            },
            
            // OOP - Multiple Choice
            new Question
            {
                Id = 5,
                Category = "OOP",
                Type = QuestionType.MultipleChoice,
                Text = "Which pillar of OOP allows hiding implementation details?",
                Choices = new List<string> { "Encapsulation", "Inheritance", "Polymorphism", "Abstraction" },
                CorrectAnswer = "Encapsulation",
                Explanation = "Encapsulation hides implementation details and controls access to data."
            },
            new Question
            {
                Id = 6,
                Category = "OOP",
                Type = QuestionType.MultipleChoice,
                Text = "Which syntax allows a class to inherit from another class in C#?",
                Choices = new List<string> { ":", "extends", "inherits", "implements" },
                CorrectAnswer = ":",
                Explanation = "In C#, ':' is used to indicate inheritance (unlike 'extends' in Java)."
            },
            new Question
            {
                Id = 7,
                Category = "OOP",
                Type = QuestionType.MultipleChoice,
                Text = "Can an abstract class be instantiated?",
                Choices = new List<string> { "No", "Yes", "Only with new", "Depends on context" },
                CorrectAnswer = "No",
                Explanation = "An abstract class cannot be instantiated directly; it must be inherited."
            },
            new Question
            {
                Id = 8,
                Category = "OOP",
                Type = QuestionType.FreeText,
                Text = "How many classes can a C# class inherit directly?",
                Choices = new List<string>(),
                CorrectAnswer = "1",
                Explanation = "C# supports only single inheritance; a class can inherit from one class."
            },

            // Collections & LINQ
            new Question
            {
                Id = 9,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which collection guarantees element uniqueness?",
                Choices = new List<string> { "HashSet", "List", "ArrayList", "Queue" },
                CorrectAnswer = "HashSet",
                Explanation = "HashSet guarantees all elements are unique."
            },
            new Question
            {
                Id = 10,
                Category = "LINQ",
                Type = QuestionType.MultipleChoice,
                Text = "Which LINQ method returns the first element or the default value?",
                Choices = new List<string> { "FirstOrDefault", "First", "Single", "Take" },
                CorrectAnswer = "FirstOrDefault",
                Explanation = "FirstOrDefault returns the first element or default(T) if the sequence is empty."
            },
            new Question
            {
                Id = 11,
                Category = "LINQ",
                Type = QuestionType.MultipleChoice,
                Text = "Which LINQ method projects each element of a sequence?",
                Choices = new List<string> { "Select", "Where", "OrderBy", "GroupBy" },
                CorrectAnswer = "Select",
                Explanation = "Select is used to project/transform each element of a sequence."
            },
            new Question
            {
                Id = 12,
                Category = "Collections",
                Type = QuestionType.FreeText,
                Text = "What is the average time complexity of a lookup in a Dictionary?",
                Choices = new List<string>(),
                CorrectAnswer = "O(1)",
                Explanation = "Dictionary uses a hash table, offering O(1) average complexity."
            },

            // Async/Await
            new Question
            {
                Id = 13,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "Which keyword marks a method as asynchronous?",
                Choices = new List<string> { "async", "await", "Task", "asynchronous" },
                CorrectAnswer = "async",
                Explanation = "The 'async' keyword marks a method as asynchronous."
            },
            new Question
            {
                Id = 14,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "What return type should an async method have?",
                Choices = new List<string> { "Task or Task<T>", "void", "async", "Promise" },
                CorrectAnswer = "Task or Task<T>",
                Explanation = "Async methods return Task, Task<T>, or void (void is discouraged except for events)."
            },
            new Question
            {
                Id = 15,
                Category = "Async",
                Type = QuestionType.MultipleChoice,
                Text = "What does the 'await' keyword do?",
                Choices = new List<string> { "Suspends execution until the task completes", "Creates a new thread", "Blocks the thread", "Cancels the task" },
                CorrectAnswer = "Suspends execution until the task completes",
                Explanation = "await suspends method execution without blocking the thread."
            },

            // Exception Handling
            new Question
            {
                Id = 16,
                Category = "Exceptions",
                Type = QuestionType.MultipleChoice,
                Text = "Which block is always executed, whether an exception is thrown or not?",
                Choices = new List<string> { "finally", "catch", "try", "throw" },
                CorrectAnswer = "finally",
                Explanation = "The finally block is always executed, even if an exception is thrown."
            },
            new Question
            {
                Id = 17,
                Category = "Exceptions",
                Type = QuestionType.MultipleChoice,
                Text = "What is the base class of all exceptions in .NET?",
                Choices = new List<string> { "Exception", "Error", "SystemException", "BaseException" },
                CorrectAnswer = "Exception",
                Explanation = "System.Exception is the base class of all exceptions in .NET."
            },

            // Delegates & Events
            new Question
            {
                Id = 18,
                Category = "Delegates",
                Type = QuestionType.MultipleChoice,
                Text = "What is a delegate in C#?",
                Choices = new List<string> { "A type that references methods", "A class", "An interface", "A value type" },
                CorrectAnswer = "A type that references methods",
                Explanation = "A delegate is a type that can reference methods with a specific signature."
            },
            new Question
            {
                Id = 19,
                Category = "Events",
                Type = QuestionType.MultipleChoice,
                Text = "Which keyword is used to declare an event?",
                Choices = new List<string> { "event", "delegate", "action", "trigger" },
                CorrectAnswer = "event",
                Explanation = "The 'event' keyword is used to declare an event based on a delegate."
            },

            // Generics
            new Question
            {
                Id = 20,
                Category = "Generics",
                Type = QuestionType.MultipleChoice,
                Text = "Which symbol is used to declare a generic type?",
                Choices = new List<string> { "<T>", "[T]", "(T)", "{T}" },
                CorrectAnswer = "<T>",
                Explanation = "Angle brackets <> are used to declare and use generic types."
            },
            new Question
            {
                Id = 21,
                Category = "Generics",
                Type = QuestionType.MultipleChoice,
                Text = "Which generic type constraint specifies that T must be a reference type?",
                Choices = new List<string> { "where T : class", "where T : struct", "where T : new()", "where T : object" },
                CorrectAnswer = "where T : class",
                Explanation = "'where T : class' constrains T to be a reference type."
            },

            // Design Patterns
            new Question
            {
                Id = 22,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "Which pattern ensures a class has only one instance?",
                Choices = new List<string> { "Singleton", "Factory", "Observer", "Strategy" },
                CorrectAnswer = "Singleton",
                Explanation = "The Singleton pattern ensures a class has only one global instance."
            },
            new Question
            {
                Id = 23,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "Which pattern allows creating objects without specifying their exact class?",
                Choices = new List<string> { "Factory", "Singleton", "Adapter", "Decorator" },
                CorrectAnswer = "Factory",
                Explanation = "The Factory pattern allows creating objects without exposing creation logic."
            },

            // .NET Framework
            new Question
            {
                Id = 24,
                Category = ".NET",
                Type = QuestionType.MultipleChoice,
                Text = "What is the CLR?",
                Choices = new List<string> { "Common Language Runtime", "Common Library Repository", "Code Language Runtime", "Class Library Reference" },
                CorrectAnswer = "Common Language Runtime",
                Explanation = "CLR (Common Language Runtime) is the execution environment of .NET."
            },
            new Question
            {
                Id = 25,
                Category = ".NET",
                Type = QuestionType.MultipleChoice,
                Text = "What is the name of .NET's garbage collector?",
                Choices = new List<string> { "GC", "MemoryManager", "Collector", "CleanUp" },
                CorrectAnswer = "GC",
                Explanation = "GC (Garbage Collector) automatically manages memory in .NET."
            },
            new Question
            {
                Id = 26,
                Category = ".NET",
                Type = QuestionType.FreeText,
                Text = "How many generations does the .NET garbage collector use?",
                Choices = new List<string>(),
                CorrectAnswer = "3",
                Explanation = "The .NET GC uses 3 generations: 0, 1, and 2."
            },

            // String & Performance
            new Question
            {
                Id = 27,
                Category = "Performance",
                Type = QuestionType.MultipleChoice,
                Text = "Which class is more efficient for concatenating many strings?",
                Choices = new List<string> { "StringBuilder", "String", "StringBuffer", "Concat" },
                CorrectAnswer = "StringBuilder",
                Explanation = "StringBuilder is optimized for multiple concatenation operations."
            },
            new Question
            {
                Id = 28,
                Category = "String",
                Type = QuestionType.MultipleChoice,
                Text = "Are strings in C# mutable or immutable?",
                Choices = new List<string> { "Immutable", "Mutable", "Depends on context", "Both" },
                CorrectAnswer = "Immutable",
                Explanation = "Strings in C# are immutable; each modification creates a new instance."
            },

            // Value Types vs Reference Types
            new Question
            {
                Id = 29,
                Category = "Types",
                Type = QuestionType.MultipleChoice,
                Text = "Where are value types stored?",
                Choices = new List<string> { "Stack", "Heap", "Cache", "Registry" },
                CorrectAnswer = "Stack",
                Explanation = "Value types are generally stored on the stack."
            },
            new Question
            {
                Id = 30,
                Category = "Types",
                Type = QuestionType.MultipleChoice,
                Text = "Which keyword defines a custom value type?",
                Choices = new List<string> { "struct", "class", "value", "type" },
                CorrectAnswer = "struct",
                Explanation = "The 'struct' keyword defines a custom value type."
            },

            // Nullable Types
            new Question
            {
                Id = 31,
                Category = "Nullable",
                Type = QuestionType.MultipleChoice,
                Text = "How do you declare a nullable int?",
                Choices = new List<string> { "int?", "Nullable<int>", "int!", "nullint" },
                CorrectAnswer = "int?",
                Explanation = "'int?' is shorthand for 'Nullable<int>'."
            },
            new Question
            {
                Id = 32,
                Category = "Nullable",
                Type = QuestionType.MultipleChoice,
                Text = "Which operator provides a default value if null?",
                Choices = new List<string> { "??", "?.", "??=", "!" },
                CorrectAnswer = "??",
                Explanation = "The '??' (null-coalescing) operator provides a default value if the operand is null."
            },

            // Interface & Abstract
            new Question
            {
                Id = 33,
                Category = "Interface",
                Type = QuestionType.MultipleChoice,
                Text = "Can an interface contain fields?",
                Choices = new List<string> { "No", "Yes", "Only constants", "Only readonly" },
                CorrectAnswer = "No",
                Explanation = "An interface cannot contain fields, only properties, methods, etc."
            },
            new Question
            {
                Id = 34,
                Category = "Interface",
                Type = QuestionType.FreeText,
                Text = "How many interfaces can a class implement?",
                Choices = new List<string>(),
                CorrectAnswer = "unlimited",
                Explanation = "A class can implement an unlimited number of interfaces."
            },

            // Access Modifiers
            new Question
            {
                Id = 35,
                Category = "Modifiers",
                Type = QuestionType.MultipleChoice,
                Text = "Which modifier makes a member accessible within the assembly?",
                Choices = new List<string> { "internal", "public", "protected", "private" },
                CorrectAnswer = "internal",
                Explanation = "The 'internal' modifier limits access to the current assembly."
            },

            // Database Connections (5)
            new Question
            {
                Id = 36,
                Category = "Database",
                Type = QuestionType.MultipleChoice,
                Text = "Which ADO.NET class is used to create a connection to SQL Server?",
                Choices = new List<string> { "SqlConnection", "SqlCommand", "SqlDataAdapter", "DbSet" },
                CorrectAnswer = "SqlConnection",
                Explanation = "SqlConnection represents a connection to a SQL Server database."
            },
            new Question
            {
                Id = 37,
                Category = "Database",
                Type = QuestionType.MultipleChoice,
                Text = "Which connection string setting enables Windows Integrated Authentication?",
                Choices = new List<string> { "Integrated Security=True", "Authentication=Windows", "TrustedProvider=SSPI", "UseWindowsAuth=True" },
                CorrectAnswer = "Integrated Security=True",
                Explanation = "Using 'Integrated Security=True' (or 'Trusted_Connection=True') enables Windows authentication."
            },
            new Question
            {
                Id = 38,
                Category = "Database",
                Type = QuestionType.MultipleChoice,
                Text = "What is the recommended pattern to ensure a connection is always closed and disposed?",
                Choices = new List<string> { "using statement", "try/catch only", "GC.Collect", "Finalize override" },
                CorrectAnswer = "using statement",
                Explanation = "The 'using' statement guarantees Dispose is called even when exceptions occur."
            },
            new Question
            {
                Id = 39,
                Category = "Database",
                Type = QuestionType.MultipleChoice,
                Text = "Which method opens an ADO.NET connection asynchronously?",
                Choices = new List<string> { "OpenAsync", "OpenTask", "BeginOpen", "Open" },
                CorrectAnswer = "OpenAsync",
                Explanation = "Use OpenAsync() to open the connection without blocking the calling thread."
            },
            new Question
            {
                Id = 40,
                Category = "Database",
                Type = QuestionType.MultipleChoice,
                Text = "What is the main benefit of connection pooling?",
                Choices = new List<string> { "Reuses physical connections", "Encrypts data by default", "Avoids deadlocks", "Caches query plans" },
                CorrectAnswer = "Reuses physical connections",
                Explanation = "Pooling reuses existing connections, reducing the overhead of frequent open/close operations."
            },

            // Entity Framework (5)
            new Question
            {
                Id = 41,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which EF Core class represents a session with the database?",
                Choices = new List<string> { "DbContext", "DbSet", "DataContextFactory", "ObjectSet" },
                CorrectAnswer = "DbContext",
                Explanation = "DbContext is the primary class for interacting with EF Core and the database."
            },
            new Question
            {
                Id = 42,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which method applies pending migrations at application startup?",
                Choices = new List<string> { "Database.Migrate()", "Database.Update()", "ApplyMigrations()", "EnsureCreated()" },
                CorrectAnswer = "Database.Migrate()",
                Explanation = "Database.Migrate() applies any pending migrations to the database."
            },
            new Question
            {
                Id = 43,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which method marks a new entity for insertion on SaveChanges()?",
                Choices = new List<string> { "Add", "Attach", "Update", "Track" },
                CorrectAnswer = "Add",
                Explanation = "Add sets the entity state to Added so EF will insert it on SaveChanges()."
            },
            new Question
            {
                Id = 44,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which LINQ method executes the query and returns results asynchronously?",
                Choices = new List<string> { "ToListAsync", "AsQueryable", "Where", "Select" },
                CorrectAnswer = "ToListAsync",
                Explanation = "ToListAsync materializes the query asynchronously."
            },
            new Question
            {
                Id = 45,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which attribute can be used to configure a primary key on a property?",
                Choices = new List<string> { "[Key]", "[Primary]", "[Id]", "[Index]" },
                CorrectAnswer = "[Key]",
                Explanation = "The [Key] attribute (System.ComponentModel.DataAnnotations) marks a property as the primary key."
            },

            // SQL (15)
            new Question
            {
                Id = 46,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which JOIN returns all rows from the left table and matches from the right (NULL when missing)?",
                Choices = new List<string> { "LEFT JOIN", "INNER JOIN", "RIGHT JOIN", "FULL JOIN" },
                CorrectAnswer = "LEFT JOIN",
                Explanation = "LEFT JOIN returns all rows from the left table and NULLs for non-matching right rows."
            },
            new Question
            {
                Id = 47,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which clause filters aggregated results?",
                Choices = new List<string> { "HAVING", "WHERE", "GROUP BY", "QUALIFY" },
                CorrectAnswer = "HAVING",
                Explanation = "HAVING filters groups after aggregation; WHERE filters rows before grouping."
            },
            new Question
            {
                Id = 48,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which approach can return the 3rd highest salary reliably across ties?",
                Choices = new List<string> { "Use DENSE_RANK() and filter WHERE rank = 3", "ORDER BY salary DESC OFFSET 2 ROWS FETCH NEXT 1 ROW ONLY", "TOP 3 with ORDER BY then MIN", "COUNT(DISTINCT salary) = 3" },
                CorrectAnswer = "Use DENSE_RANK() and filter WHERE rank = 3",
                Explanation = "Window functions like DENSE_RANK handle ties better than simple OFFSET/FETCH in ranking scenarios."
            },
            new Question
            {
                Id = 49,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which access pattern uses an index to locate rows directly by key?",
                Choices = new List<string> { "Index Seek", "Index Scan", "Table Scan", "Hash Match" },
                CorrectAnswer = "Index Seek",
                Explanation = "An index seek navigates the index b-tree to locate the requested key range efficiently."
            },
            new Question
            {
                Id = 50,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is a covering index?",
                Choices = new List<string> { "An index that contains all columns needed by a query", "The clustered index", "An index on every column", "A unique index" },
                CorrectAnswer = "An index that contains all columns needed by a query",
                Explanation = "A covering index includes all columns referenced by the query, avoiding lookups to the base table."
            },
            new Question
            {
                Id = 51,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "In ACID properties, what does 'I' stand for?",
                Choices = new List<string> { "Isolation", "Integrity", "Idempotence", "Indexing" },
                CorrectAnswer = "Isolation",
                Explanation = "Isolation ensures concurrent transactions do not interfere with each other."
            },
            new Question
            {
                Id = 52,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which isolation level prevents dirty reads but allows non-repeatable reads?",
                Choices = new List<string> { "READ COMMITTED", "READ UNCOMMITTED", "REPEATABLE READ", "SERIALIZABLE" },
                CorrectAnswer = "READ COMMITTED",
                Explanation = "READ COMMITTED prevents dirty reads; non-repeatable reads can still occur."
            },
            new Question
            {
                Id = 53,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "How can you delete duplicate rows while keeping a single row per key?",
                Choices = new List<string> { "Use ROW_NUMBER() OVER(PARTITION BY ...) and delete rows WHERE rn > 1", "Use DISTINCT in DELETE", "Use GROUP BY without WHERE", "Create a UNIQUE constraint only" },
                CorrectAnswer = "Use ROW_NUMBER() OVER(PARTITION BY ...) and delete rows WHERE rn > 1",
                Explanation = "ROW_NUMBER with a CTE lets you identify duplicates and delete those with rn > 1."
            },
            new Question
            {
                Id = 54,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is the difference between COUNT(*) and COUNT(column)?",
                Choices = new List<string> { "COUNT(*) counts all rows; COUNT(column) ignores NULLs", "COUNT(*) ignores NULLs; COUNT(column) includes NULLs", "They are always identical", "COUNT(column) is faster in all cases" },
                CorrectAnswer = "COUNT(*) counts all rows; COUNT(column) ignores NULLs",
                Explanation = "COUNT(column) does not count NULL values, whereas COUNT(*) counts all rows."
            },
            new Question
            {
                Id = 55,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which set operator returns rows from the first query that are not present in the second?",
                Choices = new List<string> { "EXCEPT", "INTERSECT", "UNION", "UNION ALL" },
                CorrectAnswer = "EXCEPT",
                Explanation = "EXCEPT returns rows present in the first query but not in the second."
            },
            new Question
            {
                Id = 56,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What does COALESCE(expr1, expr2, ...) do?",
                Choices = new List<string> { "Returns the first non-NULL expression", "Concatenates strings", "Converts to NOT NULL", "Trims whitespace" },
                CorrectAnswer = "Returns the first non-NULL expression",
                Explanation = "COALESCE evaluates arguments in order and returns the first that is not NULL."
            },
            new Question
            {
                Id = 57,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "Which feature converts row values into columns based on an aggregate?",
                Choices = new List<string> { "PIVOT", "UNPIVOT", "ROLLUP", "CUBE" },
                CorrectAnswer = "PIVOT",
                Explanation = "PIVOT rotates row data into columns for cross-tab style results."
            },
            new Question
            {
                Id = 58,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is the main purpose of a clustered index?",
                Choices = new List<string> { "Defines the physical order of rows", "Enforces uniqueness only", "Stores BLOB data", "Caches query results" },
                CorrectAnswer = "Defines the physical order of rows",
                Explanation = "A clustered index determines the table's physical row order; there can be only one per table."
            },
            new Question
            {
                Id = 59,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "In logical query processing, which clause is evaluated first?",
                Choices = new List<string> { "FROM", "WHERE", "SELECT", "ORDER BY" },
                CorrectAnswer = "FROM",
                Explanation = "Logical processing order starts with FROM (then ON, OUTER, WHERE, GROUP BY, HAVING, SELECT, ORDER BY)."
            },
            new Question
            {
                Id = 60,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is the default transaction isolation level in SQL Server?",
                Choices = new List<string> { "READ COMMITTED", "READ UNCOMMITTED", "REPEATABLE READ", "SNAPSHOT" },
                CorrectAnswer = "READ COMMITTED",
                Explanation = "By default SQL Server uses READ COMMITTED isolation."
            },

            // Fun / Humorous (5)
            new Question
            {
                Id = 61,
                Category = "Fun",
                Type = QuestionType.MultipleChoice,
                Text = "If a bug fixes itself in production, what do we call it?",
                Choices = new List<string> { "Feature", "Miracle", "Undefined Behavior", "Heisenbug" },
                CorrectAnswer = "Heisenbug",
                Explanation = "A Heisenbug disappears or alters its behavior when you try to observe it. Miracles are not in the docs."
            },
            new Question
            {
                Id = 62,
                Category = "Fun",
                Type = QuestionType.MultipleChoice,
                Text = "How many stack overflows does it take to fix a StackOverflow?",
                Choices = new List<string> { "Just one post", "Depends on recursion depth", "42", "All of them" },
                CorrectAnswer = "Depends on recursion depth",
                Explanation = "Recursion jokes aside, the best fix is usually found on the first accepted answer."
            },
            new Question
            {
                Id = 63,
                Category = "Fun",
                Type = QuestionType.MultipleChoice,
                Text = "What's the most portable programming language?",
                Choices = new List<string> { "C#", "Java", "Python", "PowerPoint" },
                CorrectAnswer = "PowerPoint",
                Explanation = "It runs everywhere—in meetings. Real answer: depends on runtime and ecosystem."
            },
            new Question
            {
                Id = 64,
                Category = "Fun",
                Type = QuestionType.MultipleChoice,
                Text = "If code works on my machine, what's the next step?",
                Choices = new List<string> { "Ship it", "Blame the network", "Write tests", "Print the DLL" },
                CorrectAnswer = "Write tests",
                Explanation = "'Works on my machine' is not a strategy—tests and reproducible environments are."
            },
            new Question
            {
                Id = 65,
                Category = "Fun",
                Type = QuestionType.MultipleChoice,
                Text = "What does 'sudo fix prod' return?",
                Choices = new List<string> { "Permission denied", "All green", "Coffee", "It depends" },
                CorrectAnswer = "It depends",
                Explanation = "It always depends—on logs, metrics, rollbacks, and whether you wrote a runbook."
            }
            ,
            // Acronyms & Concepts (AI / Protocols)
            new Question
            {
                Id = 66,
                Category = "AI",
                Type = QuestionType.MultipleChoice,
                Text = "What does the acronym LLM stand for?",
                Choices = new List<string> { "Large Language Model", "Lightweight Logic Module", "Limited Learning Machine", "Long-Lived Module" },
                CorrectAnswer = "Large Language Model",
                Explanation = "LLM stands for Large Language Model, an AI model trained on large amounts of text."
            },
            new Question
            {
                Id = 67,
                Category = "Protocols",
                Type = QuestionType.MultipleChoice,
                Text = "What does the acronym MCP mean in the context of LLMs?",
                Choices = new List<string> { "Model Context Protocol", "Managed Control Process", "Multi-Channel Pipeline", "Modular Connector Protocol" },
                CorrectAnswer = "Model Context Protocol",
                Explanation = "MCP stands for Model Context Protocol, a protocol for connecting LLMs to tools and data."
            }
            ,
            // Collections - Lists & Queues & Stacks
            new Question
            {
                Id = 68,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which statement best describes an Array in .NET?",
                Choices = new List<string> { "Fixed-size, contiguous block of elements", "Resizable, non-generic collection", "Thread-safe FIFO collection", "Doubly linked list of nodes" },
                CorrectAnswer = "Fixed-size, contiguous block of elements",
                Explanation = "An Array has a fixed length and stores elements in a contiguous block of memory."
            },
            new Question
            {
                Id = 69,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "What is a key characteristic of ArrayList?",
                Choices = new List<string> { "Non-generic, stores objects", "Generic type-safe collection", "Lock-free concurrent stack", "Immutable queue" },
                CorrectAnswer = "Non-generic, stores objects",
                Explanation = "ArrayList is a non-generic collection that stores elements as object, requiring casting."
            },
            new Question
            {
                Id = 70,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which advantage does List<T> have over ArrayList?",
                Choices = new List<string> { "Type safety with generics", "Built-in thread safety", "Constant-time resizing", "Stores elements on the stack" },
                CorrectAnswer = "Type safety with generics",
                Explanation = "List<T> is generic and provides compile-time type safety without casting."
            },
            new Question
            {
                Id = 71,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which data structure does Queue implement?",
                Choices = new List<string> { "FIFO (First-In, First-Out)", "LIFO (Last-In, First-Out)", "Binary tree", "Hash table" },
                CorrectAnswer = "FIFO (First-In, First-Out)",
                Explanation = "Queue processes items in first-in, first-out order using Enqueue and Dequeue."
            },
            new Question
            {
                Id = 72,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "What differentiates ConcurrentQueue<T> from Queue?",
                Choices = new List<string> { "Thread-safe operations for multiple producers/consumers", "Stores only reference types", "Guarantees ordering across processes", "Provides blocking Dequeue by default" },
                CorrectAnswer = "Thread-safe operations for multiple producers/consumers",
                Explanation = "ConcurrentQueue<T> offers lock-free, thread-safe operations suitable for concurrency."
            },
            new Question
            {
                Id = 73,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which data structure does Stack implement?",
                Choices = new List<string> { "LIFO (Last-In, First-Out)", "FIFO (First-In, First-Out)", "Graph", "Trie" },
                CorrectAnswer = "LIFO (Last-In, First-Out)",
                Explanation = "Stack processes items in last-in, first-out order using Push and Pop."
            },
            new Question
            {
                Id = 74,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "What is a key feature of ConcurrentStack<T>?",
                Choices = new List<string> { "Thread-safe LIFO operations", "Immutable collection", "Ordered dictionary of keys", "Blocking enqueue" },
                CorrectAnswer = "Thread-safe LIFO operations",
                Explanation = "ConcurrentStack<T> provides thread-safe push/pop operations for concurrent scenarios."
            },
            new Question
            {
                Id = 75,
                Category = "Collections",
                Type = QuestionType.MultipleChoice,
                Text = "Which statement best describes LinkedList<T>?",
                Choices = new List<string> { "Doubly linked list allowing efficient insertions/removals", "Array-based fixed-size list", "Lock-free FIFO queue", "Hash-based set of unique items" },
                CorrectAnswer = "Doubly linked list allowing efficient insertions/removals",
                Explanation = "LinkedList<T> is a doubly linked list; insertions/removals are efficient given a node reference."
            }
            ,
            // Design Patterns – Scénarios et fonctionnement
            new Question
            {
                Id = 76,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You need to create objects without exposing creation logic and return subtypes based on context. Which pattern should you use?",
                Choices = new List<string> { "Factory", "Singleton", "Adapter", "Observer" },
                CorrectAnswer = "Factory",
                Explanation = "The Factory pattern encapsulates object creation and can return different subtypes based on parameters." 
            },
            new Question
            {
                Id = 77,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You want to dynamically swap an algorithm (e.g., different sorting or pricing strategies). Which pattern should you use?",
                Choices = new List<string> { "Strategy", "Decorator", "Prototype", "Bridge" },
                CorrectAnswer = "Strategy",
                Explanation = "Strategy defines a family of interchangeable algorithms and encapsulates them for runtime selection." 
            },
            new Question
            {
                Id = 78,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You must automatically notify subscribed components when a subject's state changes. Which pattern fits?",
                Choices = new List<string> { "Observer", "Mediator", "Adapter", "Facade" },
                CorrectAnswer = "Observer",
                Explanation = "Observer lets subscribers register with a subject and receive notifications on state changes." 
            },
            new Question
            {
                Id = 79,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You must ensure a single global instance with controlled access and lazy initialization. Which pattern?",
                Choices = new List<string> { "Singleton", "Factory", "Composite", "Builder" },
                CorrectAnswer = "Singleton",
                Explanation = "Singleton guarantees a single instance and provides a global access point, often with lazy initialization." 
            },
            new Question
            {
                Id = 80,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You need to integrate an incompatible interface without changing client code. Which pattern should you apply?",
                Choices = new List<string> { "Adapter", "Decorator", "Proxy", "Bridge" },
                CorrectAnswer = "Adapter",
                Explanation = "Adapter converts a component's interface to match the one expected by the client." 
            },
            new Question
            {
                Id = 81,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You want to add responsibilities (e.g., logging, compression) to an object flexibly without changing its class. Which pattern?",
                Choices = new List<string> { "Decorator", "Proxy", "Composite", "Flyweight" },
                CorrectAnswer = "Decorator",
                Explanation = "Decorator dynamically attaches responsibilities to an object via wrappers that adhere to the same interface." 
            },
            new Question
            {
                Id = 82,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "Multiple objects must collaborate through a mediation point to reduce direct dependencies. Which pattern should you choose?",
                Choices = new List<string> { "Mediator", "Observer", "Facade", "Prototype" },
                CorrectAnswer = "Mediator",
                Explanation = "Mediator centralizes interactions among objects to reduce coupling and relational complexity." 
            },
            new Question
            {
                Id = 83,
                Category = "Design Patterns",
                Type = QuestionType.MultipleChoice,
                Text = "You want to simplify access to a complex subsystem through a single, consistent interface. Which pattern?",
                Choices = new List<string> { "Facade", "Bridge", "Composite", "Proxy" },
                CorrectAnswer = "Facade",
                Explanation = "Facade provides a unified interface that simplifies the use of a complex subsystem." 
            },

            // ASP.NET Core / Web (20 questions)
            new Question
            {
                Id = 84,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which method is used to add middleware to the ASP.NET Core pipeline?",
                Choices = new List<string> { "Use", "Map", "Run", "Add" },
                CorrectAnswer = "Use",
                Explanation = "The Use method adds middleware to the request pipeline that can pass control to the next middleware."
            },
            new Question
            {
                Id = 85,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the difference between AddScoped and AddTransient in dependency injection?",
                Choices = new List<string> { "Scoped creates one instance per request, Transient creates a new instance each time", "Scoped is thread-safe, Transient is not", "Scoped is faster than Transient", "They are identical" },
                CorrectAnswer = "Scoped creates one instance per request, Transient creates a new instance each time",
                Explanation = "AddScoped creates one instance per HTTP request, while AddTransient creates a new instance every time it's requested."
            },
            new Question
            {
                Id = 86,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which attribute is used to bind action parameters from the request body?",
                Choices = new List<string> { "[FromBody]", "[FromQuery]", "[FromRoute]", "[FromForm]" },
                CorrectAnswer = "[FromBody]",
                Explanation = "[FromBody] binds the parameter from the request body, typically for JSON payloads."
            },
            new Question
            {
                Id = 87,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What HTTP status code should be returned when a resource is successfully created?",
                Choices = new List<string> { "201 Created", "200 OK", "204 No Content", "202 Accepted" },
                CorrectAnswer = "201 Created",
                Explanation = "201 Created indicates successful resource creation and should include a Location header pointing to the new resource."
            },
            new Question
            {
                Id = 88,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which method terminates the middleware pipeline without calling the next middleware?",
                Choices = new List<string> { "Run", "Use", "Map", "UseWhen" },
                CorrectAnswer = "Run",
                Explanation = "Run is a terminal middleware that doesn't call next(), ending the pipeline."
            },
            new Question
            {
                Id = 89,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of ModelState.IsValid in ASP.NET Core?",
                Choices = new List<string> { "Validates model binding and data annotations", "Checks if the model is null", "Verifies SQL injection attacks", "Ensures the model is serializable" },
                CorrectAnswer = "Validates model binding and data annotations",
                Explanation = "ModelState.IsValid checks whether model binding succeeded and all validation attributes are satisfied."
            },
            new Question
            {
                Id = 90,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which interface should a middleware class implement?",
                Choices = new List<string> { "IMiddleware", "IApplicationMiddleware", "IRequestHandler", "IHttpMiddleware" },
                CorrectAnswer = "IMiddleware",
                Explanation = "Custom middleware classes can implement IMiddleware for factory-based activation."
            },
            new Question
            {
                Id = 91,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of the [ApiController] attribute?",
                Choices = new List<string> { "Enables automatic model validation and binding conventions", "Marks a controller as thread-safe", "Enables caching automatically", "Adds authentication requirements" },
                CorrectAnswer = "Enables automatic model validation and binding conventions",
                Explanation = "[ApiController] enables features like automatic HTTP 400 responses for invalid models and inference of binding sources."
            },
            new Question
            {
                Id = 92,
                Category = "HTTP",
                Type = QuestionType.MultipleChoice,
                Text = "Which HTTP method is idempotent and should be used to update a complete resource?",
                Choices = new List<string> { "PUT", "POST", "PATCH", "DELETE" },
                CorrectAnswer = "PUT",
                Explanation = "PUT is idempotent and replaces the entire resource. Multiple identical PUT requests have the same effect as a single one."
            },
            new Question
            {
                Id = 93,
                Category = "HTTP",
                Type = QuestionType.MultipleChoice,
                Text = "What HTTP status code indicates that the server understood the request but refuses to authorize it?",
                Choices = new List<string> { "403 Forbidden", "401 Unauthorized", "400 Bad Request", "405 Method Not Allowed" },
                CorrectAnswer = "403 Forbidden",
                Explanation = "403 Forbidden means authentication succeeded but the user lacks permission. 401 means authentication is required."
            },
            new Question
            {
                Id = 94,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which method is used to configure services in the dependency injection container?",
                Choices = new List<string> { "ConfigureServices", "Configure", "AddServices", "RegisterServices" },
                CorrectAnswer = "ConfigureServices",
                Explanation = "In older ASP.NET Core versions, ConfigureServices was used. In .NET 6+, services are added directly to builder.Services."
            },
            new Question
            {
                Id = 95,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of IActionResult in ASP.NET Core?",
                Choices = new List<string> { "Represents the result of an action method", "Validates action parameters", "Filters incoming requests", "Manages session state" },
                CorrectAnswer = "Represents the result of an action method",
                Explanation = "IActionResult is an interface for action results like OkResult, NotFoundResult, CreatedResult, etc."
            },
            new Question
            {
                Id = 96,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which attribute prevents over-posting attacks by specifying which properties can be bound?",
                Choices = new List<string> { "[Bind]", "[ValidateInput]", "[BindProperty]", "[SafeModel]" },
                CorrectAnswer = "[Bind]",
                Explanation = "The [Bind] attribute specifies which properties the model binder should include, preventing mass assignment vulnerabilities."
            },
            new Question
            {
                Id = 97,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the difference between IActionFilter and IResultFilter?",
                Choices = new List<string> { "ActionFilter runs before/after action execution, ResultFilter runs before/after result execution", "ActionFilter is synchronous, ResultFilter is asynchronous", "ActionFilter is for APIs only, ResultFilter is for MVC", "They are identical" },
                CorrectAnswer = "ActionFilter runs before/after action execution, ResultFilter runs before/after result execution",
                Explanation = "IActionFilter executes around action method execution, while IResultFilter executes around result execution (e.g., view rendering)."
            },
            new Question
            {
                Id = 98,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which method should you use to return a 404 Not Found response in a controller?",
                Choices = new List<string> { "NotFound()", "BadRequest()", "NoContent()", "StatusCode(404)" },
                CorrectAnswer = "NotFound()",
                Explanation = "NotFound() returns a 404 status code. StatusCode(404) also works but NotFound() is more semantic."
            },
            new Question
            {
                Id = 99,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is content negotiation in ASP.NET Core Web API?",
                Choices = new List<string> { "Selecting response format based on Accept header", "Compressing response data", "Validating request content", "Encrypting API responses" },
                CorrectAnswer = "Selecting response format based on Accept header",
                Explanation = "Content negotiation allows the API to return different formats (JSON, XML) based on the client's Accept header."
            },
            new Question
            {
                Id = 100,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which method configures strongly-typed configuration objects?",
                Choices = new List<string> { "Configure<T>", "AddOptions<T>", "Bind<T>", "Register<T>" },
                CorrectAnswer = "Configure<T>",
                Explanation = "services.Configure<TOptions>(configuration.GetSection(\"SectionName\")) binds configuration to strongly-typed options."
            },
            new Question
            {
                Id = 101,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of the [Authorize] attribute?",
                Choices = new List<string> { "Restricts access to authenticated users", "Validates model data", "Enables CORS", "Configures routing" },
                CorrectAnswer = "Restricts access to authenticated users",
                Explanation = "[Authorize] restricts access to authenticated users. It can also specify roles, policies, or authentication schemes."
            },
            new Question
            {
                Id = 102,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "Which HTTP method should be used for partial updates?",
                Choices = new List<string> { "PATCH", "PUT", "POST", "UPDATE" },
                CorrectAnswer = "PATCH",
                Explanation = "PATCH is designed for partial updates, while PUT replaces the entire resource."
            },
            new Question
            {
                Id = 103,
                Category = "ASP.NET Core",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of UseRouting() and UseEndpoints() in the middleware pipeline?",
                Choices = new List<string> { "UseRouting matches requests to endpoints, UseEndpoints executes them", "Both are identical and interchangeable", "UseRouting handles static files, UseEndpoints handles dynamic content", "UseRouting is for MVC, UseEndpoints is for APIs" },
                CorrectAnswer = "UseRouting matches requests to endpoints, UseEndpoints executes them",
                Explanation = "UseRouting adds route matching to the pipeline, while UseEndpoints executes the matched endpoint."
            },

            // Security & Authentication (15 questions)
            new Question
            {
                Id = 104,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What does JWT stand for?",
                Choices = new List<string> { "JSON Web Token", "Java Web Technology", "JavaScript Web Template", "Joint Web Transfer" },
                CorrectAnswer = "JSON Web Token",
                Explanation = "JWT (JSON Web Token) is a compact, URL-safe token format for securely transmitting information between parties."
            },
            new Question
            {
                Id = 105,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "Which JWT component contains the actual claims and payload data?",
                Choices = new List<string> { "Payload", "Header", "Signature", "Footer" },
                CorrectAnswer = "Payload",
                Explanation = "JWT consists of three parts: Header (metadata), Payload (claims), and Signature (verification)."
            },
            new Question
            {
                Id = 106,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the primary purpose of CORS (Cross-Origin Resource Sharing)?",
                Choices = new List<string> { "Allow or restrict cross-origin HTTP requests", "Encrypt data in transit", "Prevent SQL injection", "Manage user sessions" },
                CorrectAnswer = "Allow or restrict cross-origin HTTP requests",
                Explanation = "CORS is a security feature that controls which origins can access resources in a web application."
            },
            new Question
            {
                Id = 107,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "Which attack involves injecting malicious scripts into web pages viewed by other users?",
                Choices = new List<string> { "XSS (Cross-Site Scripting)", "CSRF (Cross-Site Request Forgery)", "SQL Injection", "DDoS" },
                CorrectAnswer = "XSS (Cross-Site Scripting)",
                Explanation = "XSS attacks inject malicious scripts that execute in users' browsers. Always encode output and sanitize input."
            },
            new Question
            {
                Id = 108,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "How should passwords be stored in a database?",
                Choices = new List<string> { "Hashed with a salt using bcrypt or Argon2", "Encrypted with AES", "Plain text", "Base64 encoded" },
                CorrectAnswer = "Hashed with a salt using bcrypt or Argon2",
                Explanation = "Passwords should be hashed (one-way) with unique salts using slow algorithms like bcrypt, not encrypted (reversible)."
            },
            new Question
            {
                Id = 109,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of an anti-forgery token in web applications?",
                Choices = new List<string> { "Prevent CSRF attacks", "Prevent XSS attacks", "Prevent SQL injection", "Encrypt form data" },
                CorrectAnswer = "Prevent CSRF attacks",
                Explanation = "Anti-forgery tokens prevent Cross-Site Request Forgery by validating that requests originate from your application."
            },
            new Question
            {
                Id = 110,
                Category = "Authentication",
                Type = QuestionType.MultipleChoice,
                Text = "What is the difference between authentication and authorization?",
                Choices = new List<string> { "Authentication verifies identity, authorization verifies permissions", "Authentication is for APIs, authorization is for web apps", "They are identical", "Authentication uses tokens, authorization uses cookies" },
                CorrectAnswer = "Authentication verifies identity, authorization verifies permissions",
                Explanation = "Authentication verifies who you are (identity), while authorization determines what you can access (permissions)."
            },
            new Question
            {
                Id = 111,
                Category = "Authentication",
                Type = QuestionType.MultipleChoice,
                Text = "Which OAuth 2.0 flow is recommended for server-side web applications?",
                Choices = new List<string> { "Authorization Code Flow", "Implicit Flow", "Client Credentials Flow", "Resource Owner Password Flow" },
                CorrectAnswer = "Authorization Code Flow",
                Explanation = "Authorization Code Flow is most secure for server-side apps as the token is never exposed to the browser."
            },
            new Question
            {
                Id = 112,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the primary defense against SQL injection attacks?",
                Choices = new List<string> { "Use parameterized queries or ORM", "Validate input length", "Use stored procedures only", "Encrypt the database" },
                CorrectAnswer = "Use parameterized queries or ORM",
                Explanation = "Parameterized queries (or ORMs like EF Core) prevent SQL injection by separating SQL code from data."
            },
            new Question
            {
                Id = 113,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What does the HttpOnly flag on a cookie prevent?",
                Choices = new List<string> { "JavaScript access to the cookie", "Cookie transmission over HTTP", "Cookie expiration", "Cookie encryption" },
                CorrectAnswer = "JavaScript access to the cookie",
                Explanation = "HttpOnly prevents client-side JavaScript from accessing the cookie, mitigating XSS-based cookie theft."
            },
            new Question
            {
                Id = 114,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What does the Secure flag on a cookie ensure?",
                Choices = new List<string> { "Cookie is only sent over HTTPS", "Cookie is encrypted", "Cookie is HttpOnly", "Cookie has anti-forgery protection" },
                CorrectAnswer = "Cookie is only sent over HTTPS",
                Explanation = "The Secure flag ensures cookies are only transmitted over encrypted HTTPS connections."
            },
            new Question
            {
                Id = 115,
                Category = "Authentication",
                Type = QuestionType.MultipleChoice,
                Text = "What is OpenID Connect (OIDC)?",
                Choices = new List<string> { "An identity layer on top of OAuth 2.0", "A database authentication protocol", "A password hashing algorithm", "A cross-origin policy" },
                CorrectAnswer = "An identity layer on top of OAuth 2.0",
                Explanation = "OpenID Connect is an identity layer built on OAuth 2.0 that adds authentication to authorization."
            },
            new Question
            {
                Id = 116,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of Content Security Policy (CSP)?",
                Choices = new List<string> { "Prevent XSS by restricting resource sources", "Encrypt HTTP traffic", "Prevent CSRF attacks", "Manage CORS policies" },
                CorrectAnswer = "Prevent XSS by restricting resource sources",
                Explanation = "CSP is a security header that restricts which sources can load scripts, styles, and other resources, mitigating XSS."
            },
            new Question
            {
                Id = 117,
                Category = "Authentication",
                Type = QuestionType.MultipleChoice,
                Text = "Which ASP.NET Core Identity method is used to sign in a user?",
                Choices = new List<string> { "SignInManager.SignInAsync", "UserManager.LoginAsync", "AuthenticationManager.SignIn", "IdentityManager.Authenticate" },
                CorrectAnswer = "SignInManager.SignInAsync",
                Explanation = "SignInManager<TUser>.SignInAsync creates the authentication cookie and signs in the user."
            },
            new Question
            {
                Id = 118,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the principle of least privilege?",
                Choices = new List<string> { "Grant minimum permissions necessary to perform a task", "Use the simplest code possible", "Store minimal data in databases", "Use the shortest passwords allowed" },
                CorrectAnswer = "Grant minimum permissions necessary to perform a task",
                Explanation = "Least privilege means users and services should have only the minimum permissions required for their function."
            },
            new Question
            {
                Id = 119,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of rate limiting in APIs?",
                Choices = new List<string> { "Prevent abuse and DoS attacks by limiting request frequency", "Improve performance", "Encrypt data", "Validate input" },
                CorrectAnswer = "Prevent abuse and DoS attacks by limiting request frequency",
                Explanation = "Rate limiting restricts the number of requests a client can make in a time period, preventing abuse and DoS."
            },
            new Question
            {
                Id = 120,
                Category = "Security",
                Type = QuestionType.MultipleChoice,
                Text = "Which header should be set to prevent clickjacking attacks?",
                Choices = new List<string> { "X-Frame-Options", "X-Content-Type-Options", "X-XSS-Protection", "Strict-Transport-Security" },
                CorrectAnswer = "X-Frame-Options",
                Explanation = "X-Frame-Options (or CSP frame-ancestors) prevents your site from being embedded in iframes, preventing clickjacking."
            },
            new Question
            {
                Id = 121,
                Category = "Authentication",
                Type = QuestionType.MultipleChoice,
                Text = "What does MFA (Multi-Factor Authentication) provide?",
                Choices = new List<string> { "Additional security layer beyond password", "Faster login process", "Automatic password reset", "Single sign-on capability" },
                CorrectAnswer = "Additional security layer beyond password",
                Explanation = "MFA requires multiple verification factors (something you know, have, or are) for enhanced security."
            },

            // Modern C# Features (10 questions)
            new Question
            {
                Id = 122,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is a record in C# 9+?",
                Choices = new List<string> { "Immutable reference type with value-based equality", "A database table representation", "An async data structure", "A logging mechanism" },
                CorrectAnswer = "Immutable reference type with value-based equality",
                Explanation = "Records are reference types with value-based equality, immutability by default, and concise syntax."
            },
            new Question
            {
                Id = 123,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What does the 'init' accessor do in C#?",
                Choices = new List<string> { "Allows property to be set during initialization only", "Initializes the property to default value", "Makes property read-only", "Enables lazy initialization" },
                CorrectAnswer = "Allows property to be set during initialization only",
                Explanation = "init accessors allow properties to be set during object initialization but become read-only afterwards."
            },
            new Question
            {
                Id = 124,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is pattern matching in C#?",
                Choices = new List<string> { "Testing if a value matches a specific shape or pattern", "Matching regular expressions", "Comparing strings", "Database query matching" },
                CorrectAnswer = "Testing if a value matches a specific shape or pattern",
                Explanation = "Pattern matching allows testing values against patterns in switch expressions, is expressions, etc."
            },
            new Question
            {
                Id = 125,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of the 'required' modifier in C# 11?",
                Choices = new List<string> { "Forces property to be initialized during construction", "Makes parameter mandatory", "Enables null checking", "Marks method as essential" },
                CorrectAnswer = "Forces property to be initialized during construction",
                Explanation = "The required modifier ensures a property must be set during object creation, enforced by the compiler."
            },
            new Question
            {
                Id = 126,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is a top-level statement in C# 9+?",
                Choices = new List<string> { "Code outside of a class or method in Program.cs", "A statement at the beginning of a method", "The first line of a class", "A global variable declaration" },
                CorrectAnswer = "Code outside of a class or method in Program.cs",
                Explanation = "Top-level statements eliminate boilerplate code by allowing statements directly in Program.cs without a Main method."
            },
            new Question
            {
                Id = 127,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What does the null-coalescing assignment operator ??= do?",
                Choices = new List<string> { "Assigns value only if left operand is null", "Checks if both operands are null", "Throws if value is null", "Converts null to default value" },
                CorrectAnswer = "Assigns value only if left operand is null",
                Explanation = "x ??= y assigns y to x only if x is null, equivalent to x = x ?? y."
            },
            new Question
            {
                Id = 128,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is a switch expression in C#?",
                Choices = new List<string> { "Concise syntax that returns a value based on patterns", "A loop structure", "An async expression", "A LINQ expression" },
                CorrectAnswer = "Concise syntax that returns a value based on patterns",
                Explanation = "Switch expressions provide a concise, expression-based alternative to switch statements with pattern matching."
            },
            new Question
            {
                Id = 129,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of the 'with' keyword for records?",
                Choices = new List<string> { "Creates a copy with modified properties", "Joins two records", "Validates record data", "Serializes the record" },
                CorrectAnswer = "Creates a copy with modified properties",
                Explanation = "The 'with' expression creates a copy of a record with specified properties changed (non-destructive mutation)."
            },
            new Question
            {
                Id = 130,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What does the null-forgiving operator '!' do?",
                Choices = new List<string> { "Tells compiler to suppress nullable warnings", "Throws if value is null", "Converts null to default", "Checks for null" },
                CorrectAnswer = "Tells compiler to suppress nullable warnings",
                Explanation = "The ! operator suppresses nullable reference type warnings when you know a value isn't null."
            },
            new Question
            {
                Id = 131,
                Category = "Modern C#",
                Type = QuestionType.MultipleChoice,
                Text = "What is a primary constructor in C# 12?",
                Choices = new List<string> { "Constructor parameters declared in class declaration", "The first constructor defined", "A required constructor", "A parameterless constructor" },
                CorrectAnswer = "Constructor parameters declared in class declaration",
                Explanation = "Primary constructors allow declaring constructor parameters directly in the class declaration for concise syntax."
            },

            // Additional SQL & EF Core (10 questions)
            new Question
            {
                Id = 132,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "What is the N+1 query problem in EF Core?",
                Choices = new List<string> { "Loading parent then querying each child separately", "Loading too much data at once", "Using wrong index", "Having null foreign keys" },
                CorrectAnswer = "Loading parent then querying each child separately",
                Explanation = "N+1 problem occurs when loading a parent entity (1 query) then loading each child in separate queries (N queries). Use Include() to fix."
            },
            new Question
            {
                Id = 133,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "Which method eagerly loads related entities in EF Core?",
                Choices = new List<string> { "Include()", "Load()", "Join()", "Fetch()" },
                CorrectAnswer = "Include()",
                Explanation = "Include() eagerly loads related entities in the same query. ThenInclude() loads nested relationships."
            },
            new Question
            {
                Id = 134,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "What is the purpose of AsNoTracking() in EF Core?",
                Choices = new List<string> { "Improves performance for read-only queries", "Disables change tracking for updates", "Prevents SQL injection", "Enables lazy loading" },
                CorrectAnswer = "Improves performance for read-only queries",
                Explanation = "AsNoTracking() disables change tracking, improving performance when you don't need to update entities."
            },
            new Question
            {
                Id = 135,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is a stored procedure?",
                Choices = new List<string> { "Precompiled SQL statements stored in database", "A cached query result", "A temporary table", "A database backup" },
                CorrectAnswer = "Precompiled SQL statements stored in database",
                Explanation = "Stored procedures are precompiled SQL code stored in the database, offering performance and security benefits."
            },
            new Question
            {
                Id = 136,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is a database transaction?",
                Choices = new List<string> { "A unit of work that is atomic, consistent, isolated, and durable", "A single SQL query", "A database backup operation", "A table join operation" },
                CorrectAnswer = "A unit of work that is atomic, consistent, isolated, and durable",
                Explanation = "Transactions ensure ACID properties: all operations succeed or all fail together, maintaining data integrity."
            },
            new Question
            {
                Id = 137,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "What is a migration in EF Core?",
                Choices = new List<string> { "Code that evolves database schema over time", "Moving data between tables", "Importing data from files", "Database backup" },
                CorrectAnswer = "Code that evolves database schema over time",
                Explanation = "Migrations are version-controlled changes to database schema that can be applied or rolled back."
            },
            new Question
            {
                Id = 138,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is the difference between DELETE and TRUNCATE?",
                Choices = new List<string> { "DELETE is logged and can have WHERE clause, TRUNCATE is faster but removes all rows", "DELETE is faster", "TRUNCATE can have WHERE clause", "They are identical" },
                CorrectAnswer = "DELETE is logged and can have WHERE clause, TRUNCATE is faster but removes all rows",
                Explanation = "DELETE removes rows one by one with logging and WHERE support. TRUNCATE quickly removes all rows, resets identity, minimal logging."
            },
            new Question
            {
                Id = 139,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is a database index?",
                Choices = new List<string> { "Data structure that improves query performance", "A primary key", "A foreign key relationship", "A table constraint" },
                CorrectAnswer = "Data structure that improves query performance",
                Explanation = "Indexes speed up data retrieval at the cost of slower writes and additional storage."
            },
            new Question
            {
                Id = 140,
                Category = "Entity Framework",
                Type = QuestionType.MultipleChoice,
                Text = "What does the virtual keyword enable in EF Core navigation properties?",
                Choices = new List<string> { "Lazy loading", "Eager loading", "Explicit loading", "No tracking" },
                CorrectAnswer = "Lazy loading",
                Explanation = "Making navigation properties virtual enables lazy loading proxies to load related data on first access."
            },
            new Question
            {
                Id = 141,
                Category = "SQL",
                Type = QuestionType.MultipleChoice,
                Text = "What is a foreign key constraint?",
                Choices = new List<string> { "Enforces referential integrity between tables", "Improves query performance", "Creates an index automatically", "Encrypts column data" },
                CorrectAnswer = "Enforces referential integrity between tables",
                Explanation = "Foreign key constraints ensure that values in one table match values in another, maintaining referential integrity."
            }
        };
    }

    private static List<Question> Shuffle(List<Question> source)
    {
        var list = source.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
