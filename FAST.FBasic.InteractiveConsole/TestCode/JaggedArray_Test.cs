using FAST.FBasicInterpreter;

namespace FAST.FBasicInteractiveConsole.TestCode
{
    internal class JaggedArray_Test
    {
        public void Run()
        {
            FBasicArray arr = new(){ KeepOrderOnDeleteRow=false };

            arr[0, 0] = new Value("0,0");
            arr[0, 1] = new Value("0,1");
            arr[0, 2] = new Value("0,2");
            arr[0, 3] = new Value("0,3");

            arr[1, 0] = new Value("1,0");
            arr[1, 1] = new Value("1,1");
            arr[1, 2] = new Value("1,2");
            arr[1, 3] = new Value("1,3");

            arr[2, 0] = new Value("2,0");
            arr[2, 1] = new Value("2,1");
            arr[2, 2] = new Value("2,2");
            arr[2, 3] = new Value("2,3");

            arr[3, 0] = new Value("3,0");
            arr[3, 1] = new Value("3,1");
            arr[3, 2] = new Value("3,2");
            arr[3, 3] = new Value("3,3");

            arr.DeleteRow(2);
        }
    }
}
