using static System.Math;
namespace TeleprompterConsole;

internal class Program
{
    // Entry point of program
    static void Main(string[] args)
    {
        // Calls RunTeleprompter which returns a Task and waits for 
        // the completed Task before terminating console application
        RunTeleprompter().Wait();
    }

    static IEnumerable<string> ReadFrom(string file)
    {
        // Nullable line variable
        string? line;

        // Creates limited scope for compile-time-typed variable, reader, 
        // which opens inputted file for reading.
        using (var reader = File.OpenText(file))
        {
            // While the next line being read exists
            while ((line = reader.ReadLine()) != null)
            {
                // Array of words is created from line
                var words = line.Split(' ');
                var lineLength = 0;

                // Iterate through the words
                foreach (var word in words)
                {
                    // Add word to iterator object to be returned and continue executing code
                    yield return word + " ";
                    lineLength += word.Length + 1;
                    
                    // When the length of the currently output line is longer than 70 characters, 
                    // return a "/n" to the iterator object to signify a new line should be output
                    if (lineLength > 70)
                    {
                        yield return Environment.NewLine;
                        lineLength = 0;
                    }
                }
                // Return a "/n" to the iterator object to signify a new line should be output
                yield return Environment.NewLine;
            }
        }
    }

    // Returns completed Task to RunTeleprompter when done
    private static async Task ShowTeleprompter(TelePrompterConfig config)
    {
        // Creates an iterator object from ReadFrom
        var words = ReadFrom("sampleQuotes.txt");

        // Iterates through words
        foreach (var word in words)
        {
            Console.Write(word);

            // Between every word, a delay of length config.DelayInMilliseconds is taken
            if (!string.IsNullOrWhiteSpace(word))
            {
                await Task.Delay(config.DelayInMilliseconds);
            }
        }
        // When txt file is exhausted, Done property of config is set to True
        config.SetDone();
    }

    private static async Task GetInput(TelePrompterConfig config)
    {
        // Action delegate encapsulates parameterless, void function, work
        Action work = () =>
        {
            // Continues to operate until config.Done is set to True
            do {
                // Obtains next character pressed by user
                var key = Console.ReadKey(true);
                // Speeds,
                if (key.KeyChar == '>')
                    config.UpdateDelay(-10);
                // slows,
                else if (key.KeyChar == '<')
                    config.UpdateDelay(10);
                // and terminates teleprompter output
                else if (key.KeyChar == 'X' || key.KeyChar == 'x')
                    config.SetDone();
            } while (!config.Done);
        };

        // Used Action delegate to submit work to Task.Run
        await Task.Run(work);
    }
    // Creates an instance of TelePrompterConfig (config) and passes it to ShowTeleprompter and GetInput
    private static async Task RunTeleprompter()
    {
        var config = new TelePrompterConfig();
        var displayTask = ShowTeleprompter(config);
        var speedTask = GetInput(config);
        // Termination of either one of these methods completes the task and returns completed Task to Main
        await Task.WhenAny(displayTask, speedTask);
    }
}

// 
internal class TelePrompterConfig
{
    // Delay between each output to console in ms
    public int DelayInMilliseconds { get; private set; } = 200;

    // Adjusts delay based on quantity and sign of increment
    public void UpdateDelay(int increment) // negative to speed up
    {
        var newDelay = Min(DelayInMilliseconds + increment, 1000);
        newDelay = Max(newDelay, 20);
        DelayInMilliseconds = newDelay;
    }

    // GetInput changes to True when user enters "X"/"x"
    // ShowTeleprompter changes to True when no more text is left
    public bool Done { get; private set; }
    public void SetDone()
    {
        Done = true;
    }
}

