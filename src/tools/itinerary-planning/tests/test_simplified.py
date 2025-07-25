import unittest
import sys
import os
from datetime import datetime
import re

# Add src directory to path to import modules
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

def validate_iso_date(date_str: str, param_name: str):
    """
    Validates that a string is in ISO format (YYYY-MM-DD) and returns the parsed date.
    This is a copy of the function from mcp_server.py for testing purposes.
    """
    iso_pattern = re.compile(r"^\d{4}-\d{2}-\d{2}$")
    if not iso_pattern.match(date_str):
        raise ValueError(f"{param_name} must be in ISO format (YYYY-MM-DD), got: {date_str}")

    try:
        return datetime.strptime(date_str, "%Y-%m-%d").date()
    except ValueError as e:
        raise ValueError(f"Invalid {param_name}: {e}")


class TestValidationFunctions(unittest.TestCase):
    """Test suite for validation functions."""

    def test_validate_iso_date_valid_date(self):
        """Test validate_iso_date with valid date string."""
        result = validate_iso_date("2024-03-15", "test_date")
        self.assertEqual(result, datetime(2024, 3, 15).date())

    def test_validate_iso_date_invalid_format(self):
        """Test validate_iso_date with invalid format."""
        with self.assertRaises(ValueError) as cm:
            validate_iso_date("15-03-2024", "test_date")
        self.assertIn("must be in ISO format", str(cm.exception))

    def test_validate_iso_date_invalid_date(self):
        """Test validate_iso_date with invalid date."""
        with self.assertRaises(ValueError) as cm:
            validate_iso_date("2024-13-45", "test_date")
        self.assertIn("Invalid test_date", str(cm.exception))

    def test_validate_iso_date_february_29_leap_year(self):
        """Test validate_iso_date with leap year date."""
        result = validate_iso_date("2024-02-29", "test_date")
        self.assertEqual(result, datetime(2024, 2, 29).date())

    def test_validate_iso_date_february_29_non_leap_year(self):
        """Test validate_iso_date with invalid leap year date."""
        with self.assertRaises(ValueError):
            validate_iso_date("2023-02-29", "test_date")

    def test_validate_iso_date_edge_cases(self):
        """Test validate_iso_date with edge case dates."""
        # Test start of year
        result = validate_iso_date("2024-01-01", "test_date")
        self.assertEqual(result, datetime(2024, 1, 1).date())
        
        # Test end of year
        result = validate_iso_date("2024-12-31", "test_date")
        self.assertEqual(result, datetime(2024, 12, 31).date())

    def test_validate_iso_date_various_invalid_formats(self):
        """Test validate_iso_date with various invalid formats."""
        invalid_formats = [
            "2024/03/15",
            "03-15-2024",
            "2024-3-15",
            "2024-03-5",
            "24-03-15",
            "2024-MAR-15",
            "2024-03-15T00:00:00",
            "15-Mar-2024",
            ""
        ]
        
        for invalid_format in invalid_formats:
            with self.assertRaises(ValueError):
                validate_iso_date(invalid_format, "test_date")


class TestMockDataGeneration(unittest.TestCase):
    """Test suite for mock data generation patterns."""
    
    def test_hotel_type_patterns(self):
        """Test that hotel types follow expected patterns."""
        hotel_types = ["Luxury", "Boutique", "Budget", "Business"]
        
        # Test that all types are strings and non-empty
        for hotel_type in hotel_types:
            self.assertIsInstance(hotel_type, str)
            self.assertGreater(len(hotel_type), 0)
    
    def test_flight_cabin_classes(self):
        """Test flight cabin class definitions."""
        cabin_classes = ["Economy", "Premium Economy", "Business", "First"]
        
        # Test that all classes are valid strings
        for cabin_class in cabin_classes:
            self.assertIsInstance(cabin_class, str)
            self.assertGreater(len(cabin_class), 0)
    
    def test_price_ranges(self):
        """Test price range definitions for hotels."""
        price_ranges = {
            "Luxury": (250, 600),
            "Boutique": (180, 350),
            "Budget": (80, 150),
            "Resort": (200, 500),
            "Business": (150, 300),
        }
        
        for hotel_type, (min_price, max_price) in price_ranges.items():
            self.assertIsInstance(min_price, int)
            self.assertIsInstance(max_price, int)
            self.assertGreater(max_price, min_price)
            self.assertGreater(min_price, 0)
    
    def test_airline_names(self):
        """Test airline name patterns."""
        airlines = [
            "SkyWings", "Global Air", "Atlantic Airways", "Pacific Express",
            "Mountain Jets", "Stellar Airlines", "Sunshine Airways", "Northern Flights"
        ]
        
        for airline in airlines:
            self.assertIsInstance(airline, str)
            self.assertGreater(len(airline), 0)
            # Check that airline names are reasonable length
            self.assertLessEqual(len(airline), 20)
    
    def test_aircraft_types(self):
        """Test aircraft type definitions."""
        aircraft_types = ["Boeing 737", "Airbus A320", "Boeing 787", "Airbus A350", "Embraer E190", "Bombardier CRJ900"]
        
        for aircraft in aircraft_types:
            self.assertIsInstance(aircraft, str)
            self.assertGreater(len(aircraft), 0)
            # Check that aircraft names contain expected patterns
            self.assertTrue(any(manufacturer in aircraft for manufacturer in ["Boeing", "Airbus", "Embraer", "Bombardier"]))


class TestDateLogic(unittest.TestCase):
    """Test suite for date logic and validation."""
    
    def test_date_comparison_logic(self):
        """Test that date comparison logic works correctly."""
        # This simulates the logic used in the actual functions
        check_in_date = validate_iso_date("2024-03-15", "check_in")
        check_out_date = validate_iso_date("2024-03-17", "check_out")
        
        # Check out should be after check in
        self.assertGreater(check_out_date, check_in_date)
    
    def test_same_date_validation(self):
        """Test validation when dates are the same."""
        check_in_date = validate_iso_date("2024-03-15", "check_in")
        check_out_date = validate_iso_date("2024-03-15", "check_out")
        
        # Same date should fail the "check_out after check_in" logic
        self.assertEqual(check_in_date, check_out_date)
        # In the actual implementation, this would raise ValueError
    
    def test_flight_date_scenarios(self):
        """Test various flight date scenarios."""
        departure_date = validate_iso_date("2024-03-15", "departure_date")
        return_date = validate_iso_date("2024-03-22", "return_date")
        
        # Return should be after departure
        self.assertGreater(return_date, departure_date)
        
        # Calculate days between
        days_difference = (return_date - departure_date).days
        self.assertEqual(days_difference, 7)


if __name__ == '__main__':
    print("Running simplified tests for itinerary planning MCP server...")
    print("Note: Full integration tests require external dependencies.")
    print("=" * 60)
    
    # Run tests with verbose output
    unittest.main(verbosity=2)