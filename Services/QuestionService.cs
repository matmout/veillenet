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
