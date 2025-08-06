import pytest
import sys
import os
from datetime import datetime, date
from unittest.mock import patch

# Add src directory to path for imports
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from mcp_server import suggest_hotels, suggest_flights, validate_iso_date


class TestValidateIsoDate:
    """Tests for the validate_iso_date function"""
    
    def test_valid_iso_date(self):
        """Test with a valid ISO date string"""
        result = validate_iso_date("2024-12-25", "test_date")
        expected = datetime.strptime("2024-12-25", "%Y-%m-%d").date()
        assert result == expected
    
    def test_invalid_format(self):
        """Test with invalid date format"""
        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("25-12-2024", "test_date")
    
    def test_invalid_date_values(self):
        """Test with invalid date values"""
        with pytest.raises(ValueError, match="Invalid test_date"):
            validate_iso_date("2024-13-45", "test_date")
    
    def test_partial_date(self):
        """Test with partial date string"""
        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("2024-12", "test_date")
    
    def test_empty_string(self):
        """Test with empty string"""
        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("", "test_date")


class TestSuggestHotels:
    """Tests for the suggest_hotels function"""
    
    @pytest.mark.asyncio
    async def test_valid_hotel_search(self):
        """Test hotel search with valid parameters"""
        result = await suggest_hotels(
            location="Paris",
            check_in="2024-12-20",
            check_out="2024-12-25"
        )
        
        assert isinstance(result, list)
        assert 3 <= len(result) <= 8  # Should return 3-8 hotels
        
        # Verify first hotel structure
        hotel = result[0]
        required_fields = [
            "name", "address", "location", "rating", "price_per_night", 
            "hotel_type", "amenities", "available_rooms"
        ]
        for field in required_fields:
            assert field in hotel
        
        # Verify data types and ranges
        assert isinstance(hotel["rating"], float)
        assert 3.0 <= hotel["rating"] <= 5.0
        assert isinstance(hotel["price_per_night"], (int, float))
        assert hotel["price_per_night"] > 0
        assert isinstance(hotel["amenities"], list)
        assert len(hotel["amenities"]) >= 3
        assert isinstance(hotel["available_rooms"], int)
        assert hotel["available_rooms"] >= 1
    
    @pytest.mark.asyncio
    async def test_invalid_check_in_date(self):
        """Test with invalid check-in date format"""
        with pytest.raises(ValueError, match="check_in must be in ISO format"):
            await suggest_hotels(
                location="London",
                check_in="20/12/2024",
                check_out="2024-12-25"
            )
    
    @pytest.mark.asyncio
    async def test_invalid_check_out_date(self):
        """Test with invalid check-out date format"""
        with pytest.raises(ValueError, match="check_out must be in ISO format"):
            await suggest_hotels(
                location="London",
                check_in="2024-12-20",
                check_out="25-12-2024"
            )
    
    @pytest.mark.asyncio
    async def test_check_out_before_check_in(self):
        """Test with check-out date before check-in date"""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Berlin",
                check_in="2024-12-25",
                check_out="2024-12-20"
            )
    
    @pytest.mark.asyncio
    async def test_same_check_in_and_check_out(self):
        """Test with same check-in and check-out dates"""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Madrid",
                check_in="2024-12-20",
                check_out="2024-12-20"
            )
    
    @pytest.mark.asyncio
    async def test_hotel_rating_range(self):
        """Test that all hotel ratings are within expected range"""
        result = await suggest_hotels(
            location="Rome",
            check_in="2024-12-20",
            check_out="2024-12-25"
        )
        
        for hotel in result:
            assert 3.0 <= hotel["rating"] <= 5.0
    
    @pytest.mark.asyncio
    async def test_hotel_types(self):
        """Test that hotel types are from expected list"""
        result = await suggest_hotels(
            location="Tokyo",
            check_in="2024-12-20",
            check_out="2024-12-25"
        )
        
        expected_types = ["Luxury", "Boutique", "Budget", "Business"]
        for hotel in result:
            assert hotel["hotel_type"] in expected_types
    
    @pytest.mark.asyncio
    async def test_location_inclusion_in_hotel_address(self):
        """Test that the location appears in hotel address/location"""
        location = "Barcelona"
        result = await suggest_hotels(
            location=location,
            check_in="2024-12-20",
            check_out="2024-12-25"
        )
        
        for hotel in result:
            assert location in hotel["location"]


class TestSuggestFlights:
    """Tests for the suggest_flights function"""
    
    @pytest.mark.asyncio
    async def test_one_way_flight_search(self):
        """Test one-way flight search"""
        result = await suggest_flights(
            from_location="New York",
            to_location="Los Angeles",
            departure_date="2024-12-20"
        )
        
        assert "departure_flights" in result
        assert "return_flights" in result
        assert isinstance(result["departure_flights"], list)
        assert isinstance(result["return_flights"], list)
        assert 3 <= len(result["departure_flights"]) <= 7
        assert len(result["return_flights"]) == 0  # No return flights for one-way
        
        # Verify flight structure
        flight = result["departure_flights"][0]
        required_fields = [
            "flight_id", "airline", "flight_number", "aircraft",
            "from_airport", "to_airport", "departure", "arrival",
            "duration_minutes", "is_direct", "price", "currency",
            "available_seats", "cabin_class"
        ]
        for field in required_fields:
            assert field in flight
    
    @pytest.mark.asyncio
    async def test_round_trip_flight_search(self):
        """Test round-trip flight search"""
        result = await suggest_flights(
            from_location="London",
            to_location="Paris",
            departure_date="2024-12-20",
            return_date="2024-12-25"
        )
        
        assert "departure_flights" in result
        assert "return_flights" in result
        assert 3 <= len(result["departure_flights"]) <= 7
        assert 3 <= len(result["return_flights"]) <= 7
    
    @pytest.mark.asyncio
    async def test_flight_duration_valid(self):
        """Test that flight durations are reasonable"""
        result = await suggest_flights(
            from_location="Berlin",
            to_location="Rome",
            departure_date="2024-12-20"
        )
        
        for flight in result["departure_flights"]:
            # Duration should be between 1 and 8 hours (60-480 minutes)
            assert 60 <= flight["duration_minutes"] <= 480
    
    @pytest.mark.asyncio
    async def test_flight_pricing(self):
        """Test flight pricing is reasonable"""
        result = await suggest_flights(
            from_location="Madrid",
            to_location="Barcelona",
            departure_date="2024-12-20"
        )
        
        for flight in result["departure_flights"]:
            assert isinstance(flight["price"], (int, float))
            assert 99 <= flight["price"] <= 999
            assert flight["currency"] == "USD"
    
    @pytest.mark.asyncio
    async def test_cabin_classes(self):
        """Test that cabin classes are from expected list"""
        result = await suggest_flights(
            from_location="Amsterdam",
            to_location="Vienna",
            departure_date="2024-12-20"
        )
        
        expected_classes = ["Economy", "Premium Economy", "Business", "First"]
        for flight in result["departure_flights"]:
            assert flight["cabin_class"] in expected_classes
    
    @pytest.mark.asyncio
    async def test_invalid_departure_date(self):
        """Test with invalid departure date format"""
        with pytest.raises(ValueError, match="departure_date must be in ISO format"):
            await suggest_flights(
                from_location="Chicago",
                to_location="Miami",
                departure_date="20/12/2024"
            )
    
    @pytest.mark.asyncio
    async def test_invalid_return_date(self):
        """Test with invalid return date format"""
        with pytest.raises(ValueError, match="return_date must be in ISO format"):
            await suggest_flights(
                from_location="Seattle",
                to_location="Denver",
                departure_date="2024-12-20",
                return_date="25/12/2024"
            )
    
    @pytest.mark.asyncio
    async def test_return_before_departure(self):
        """Test with return date before departure date"""
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(
                from_location="Phoenix",
                to_location="Dallas",
                departure_date="2024-12-25",
                return_date="2024-12-20"
            )
    
    @pytest.mark.asyncio
    async def test_same_departure_and_return_date(self):
        """Test with same departure and return dates"""
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(
                from_location="Atlanta",
                to_location="Houston",
                departure_date="2024-12-20",
                return_date="2024-12-20"
            )
    
    @pytest.mark.asyncio
    async def test_flight_times_valid(self):
        """Test that departure times are within expected range (6 AM to 10 PM)"""
        result = await suggest_flights(
            from_location="San Francisco",
            to_location="Las Vegas",
            departure_date="2024-12-20"
        )
        
        for flight in result["departure_flights"]:
            departure_time = datetime.fromisoformat(flight["departure"])
            assert 6 <= departure_time.hour <= 22
    
    @pytest.mark.asyncio
    async def test_connecting_flights_structure(self):
        """Test that connecting flights have proper segment structure"""
        # Run multiple times to increase chance of getting connecting flights
        for _ in range(5):
            result = await suggest_flights(
                from_location="Toronto",
                to_location="Vancouver",
                departure_date="2024-12-20"
            )
            
            connecting_flights = [f for f in result["departure_flights"] if not f["is_direct"]]
            if connecting_flights:
                flight = connecting_flights[0]
                assert "segments" in flight
                assert "connection_airport" in flight
                assert "connection_duration_minutes" in flight
                assert len(flight["segments"]) == 2
                
                # Verify segment structure
                for segment in flight["segments"]:
                    required_segment_fields = [
                        "flight_number", "from_airport", "to_airport",
                        "departure", "arrival", "duration_minutes"
                    ]
                    for field in required_segment_fields:
                        assert field in segment
                break
    
    @pytest.mark.asyncio
    async def test_available_seats_reasonable(self):
        """Test that available seats count is reasonable"""
        result = await suggest_flights(
            from_location="Boston",
            to_location="Washington",
            departure_date="2024-12-20"
        )
        
        for flight in result["departure_flights"]:
            assert 1 <= flight["available_seats"] <= 30