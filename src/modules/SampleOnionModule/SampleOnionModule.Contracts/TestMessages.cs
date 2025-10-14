namespace SampleOnionModule.Contracts;

public record AddRequest(int X, int Y);

public record AddResponse(int Sum);

public record PublishRequestMessage(int A);

public record SendRequestMessage(int A);
