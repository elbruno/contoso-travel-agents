import pytest
from datetime import datetime, date
from src.mcp_server import validate_iso_date, suggest_hotels, suggest_flights


class TestValidateIsoDate:
    """Test cases for the validate_iso_date function."""

    def test_valid_iso_date(self):
        """Test with a valid ISO date string."""
        date_str = "2024-03-15"
        result = validate_iso_date(date_str, "test_date")
        expected = date(2024, 3, 15)
        assert result == expected

    def test_invalid_format_raises_error(self):
        """Test that invalid format raises ValueError."""
        invalid_dates = [
            "2024-3-15",    # Single digit month
            "2024/03/15",   # Wrong separator
            "15-03-2024",   # Wrong order
            "2024-03",      # Missing day
            "not-a-date",   # Not a date
            "",             # Empty string
            "2024-13-01",   # Invalid month
            "2024-02-30"    # Invalid day for February
        ]
        
        for invalid_date in invalid_dates:
            with pytest.raises(ValueError):
                validate_iso_date(invalid_date, "test_date")

    def test_leap_year_february(self):
        """Test that leap year February 29 is valid."""
        result = validate_iso_date("2024-02-29", "leap_date")
        expected = date(2024, 2, 29)
        assert result == expected

    def test_non_leap_year_february(self):
        """Test that non-leap year February 29 raises error."""
        with pytest.raises(ValueError):
            validate_iso_date("2023-02-29", "non_leap_date")

    def test_boundary_dates(self):
        """Test boundary dates like start/end of year."""
        # Start of year
        result1 = validate_iso_date("2024-01-01", "start_year")
        assert result1 == date(2024, 1, 1)
        
        # End of year
        result2 = validate_iso_date("2024-12-31", "end_year")
        assert result2 == date(2024, 12, 31)


class TestSuggestHotels:
    """Test cases for the suggest_hotels function."""

    @pytest.mark.asyncio
    async def test_suggest_hotels_valid_input(self):
        """Test hotel suggestions with valid input."""
        result = await suggest_hotels(
            location="Paris",
            check_in="2024-06-01",
            check_out="2024-06-05"
        )
        
        assert isinstance(result, list)
        assert len(result) >= 3  # Should return at least 3 hotels
        assert len(result) <= 8  # Should return at most 8 hotels
        
        # Check structure of first hotel
        hotel = result[0]
        required_keys = ["name", "address", "location", "rating", "price_per_night", 
                        "hotel_type", "amenities", "available_rooms"]
        for key in required_keys:
            assert key in hotel
        
        # Check data types and ranges
        assert isinstance(hotel["rating"], float)
        assert 3.0 <= hotel["rating"] <= 5.0
        assert isinstance(hotel["price_per_night"], (int, float))
        assert hotel["price_per_night"] > 0
        assert isinstance(hotel["amenities"], list)
        assert len(hotel["amenities"]) >= 3
        assert isinstance(hotel["available_rooms"], int)
        assert hotel["available_rooms"] >= 1

    @pytest.mark.asyncio
    async def test_suggest_hotels_invalid_dates(self):
        """Test hotel suggestions with invalid date formats."""
        with pytest.raises(ValueError):
            await suggest_hotels(
                location="Paris",
                check_in="invalid-date",
                check_out="2024-06-05"
            )

    @pytest.mark.asyncio
    async def test_suggest_hotels_checkout_before_checkin(self):
        """Test that checkout before checkin raises error."""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Paris",
                check_in="2024-06-05",
                check_out="2024-06-01"
            )

    @pytest.mark.asyncio
    async def test_suggest_hotels_same_dates(self):
        """Test that same checkin and checkout dates raise error."""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Paris",
                check_in="2024-06-01",
                check_out="2024-06-01"
            )

    @pytest.mark.asyncio
    async def test_suggest_hotels_sorted_by_rating(self):
        """Test that hotels are sorted by rating (descending)."""
        result = await suggest_hotels(
            location="Tokyo",
            check_in="2024-07-01",
            check_out="2024-07-05"
        )
        
        ratings = [hotel["rating"] for hotel in result]
        assert ratings == sorted(ratings, reverse=True)

    @pytest.mark.asyncio
    async def test_suggest_hotels_location_in_result(self):
        """Test that location appears in hotel location field."""
        location = "Barcelona"
        result = await suggest_hotels(
            location=location,
            check_in="2024-08-01",
            check_out="2024-08-05"
        )
        
        for hotel in result:
            assert location in hotel["location"]


class TestSuggestFlights:
    """Test cases for the suggest_flights function."""

    @pytest.mark.asyncio
    async def test_suggest_flights_one_way(self):
        """Test flight suggestions for one-way trip."""
        result = await suggest_flights(
            from_location="New York",
            to_location="London",
            departure_date="2024-09-15"
        )
        
        assert "departure_flights" in result
        assert "return_flights" in result
        assert isinstance(result["departure_flights"], list)
        assert isinstance(result["return_flights"], list)
        assert len(result["departure_flights"]) >= 3
        assert len(result["return_flights"]) == 0  # No return flights for one-way

    @pytest.mark.asyncio
    async def test_suggest_flights_round_trip(self):
        """Test flight suggestions for round trip."""
        result = await suggest_flights(
            from_location="New York",
            to_location="London",
            departure_date="2024-09-15",
            return_date="2024-09-22"
        )
        
        assert len(result["departure_flights"]) >= 3
        assert len(result["return_flights"]) >= 3

    @pytest.mark.asyncio
    async def test_suggest_flights_invalid_departure_date(self):
        """Test that invalid departure date raises error."""
        with pytest.raises(ValueError):
            await suggest_flights(
                from_location="Paris",
                to_location="Rome",
                departure_date="invalid-date"
            )

    @pytest.mark.asyncio
    async def test_suggest_flights_return_before_departure(self):
        """Test that return date before departure raises error."""
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(
                from_location="Paris",
                to_location="Rome",
                departure_date="2024-09-15",
                return_date="2024-09-10"
            )

    @pytest.mark.asyncio
    async def test_suggest_flights_structure(self):
        """Test flight data structure."""
        result = await suggest_flights(
            from_location="Tokyo",
            to_location="Seoul",
            departure_date="2024-10-01"
        )
        
        flight = result["departure_flights"][0]
        required_keys = [
            "flight_id", "airline", "flight_number", "aircraft",
            "from_airport", "to_airport", "departure", "arrival",
            "duration_minutes", "is_direct", "price", "currency",
            "available_seats", "cabin_class"
        ]
        
        for key in required_keys:
            assert key in flight
        
        # Check airport structure
        assert "code" in flight["from_airport"]
        assert "name" in flight["from_airport"]
        assert "city" in flight["from_airport"]
        
        # Check data types
        assert isinstance(flight["duration_minutes"], int)
        assert flight["duration_minutes"] > 0
        assert isinstance(flight["price"], (int, float))
        assert flight["price"] > 0
        assert isinstance(flight["available_seats"], int)
        assert flight["available_seats"] > 0

    @pytest.mark.asyncio
    async def test_suggest_flights_direct_vs_connecting(self):
        """Test that flights can be either direct or connecting."""
        result = await suggest_flights(
            from_location="Los Angeles",
            to_location="Sydney",
            departure_date="2024-11-01"
        )
        
        # Check that we have a mix of direct and connecting flights (probabilistic)
        flights = result["departure_flights"]
        direct_flights = [f for f in flights if f["is_direct"]]
        connecting_flights = [f for f in flights if not f["is_direct"]]
        
        # Connecting flights should have segments and connection info
        for flight in connecting_flights:
            assert "segments" in flight
            assert "connection_airport" in flight
            assert "connection_duration_minutes" in flight
            assert isinstance(flight["segments"], list)
            assert len(flight["segments"]) == 2

    @pytest.mark.asyncio
    async def test_suggest_flights_datetime_format(self):
        """Test that flight times are in correct ISO format."""
        result = await suggest_flights(
            from_location="Berlin",
            to_location="Moscow",
            departure_date="2024-12-01"
        )
        
        flight = result["departure_flights"][0]
        
        # Should be able to parse departure and arrival times
        departure = datetime.fromisoformat(flight["departure"])
        arrival = datetime.fromisoformat(flight["arrival"])
        
        # Arrival should be after departure
        assert arrival > departure
        
        # Duration should approximately match the time difference
        time_diff_minutes = (arrival - departure).total_seconds() / 60
        assert abs(time_diff_minutes - flight["duration_minutes"]) < 60  # Allow 1-hour tolerance