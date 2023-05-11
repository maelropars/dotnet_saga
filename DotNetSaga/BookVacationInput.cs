namespace DotNetSaga;

public record BookVacationInput(string BookUserId, string BookCarId, string BookHotelId, string BookFlightId, int Attempts);
