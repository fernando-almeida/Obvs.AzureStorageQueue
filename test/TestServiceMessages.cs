using Obvs.Types;

namespace Obvs.AzureStorageQueue.Tests
{
    public interface  ITestServiceMessage: IMessage {

    }

    public class TestMessage: ITestServiceMessage {
        
    }

    public class TestCommand : TestMessage, ICommand { }

    public class TestCommand1 : TestCommand { }

    public class TestCommand2 : TestCommand { }
    public class TestCommand3 : TestCommand {
        public string Content {get; set;}
    }


    public class TestEvent : TestMessage, IEvent { }

    public class TestRequest : TestMessage, IRequest
    {
        public string RequestId { get; set; }
        public string RequesterId { get; set; }
    }

    public class TestResponse : TestMessage, IResponse
    {
        public string RequestId { get; set; }
        public string RequesterId { get; set; }
    }

}