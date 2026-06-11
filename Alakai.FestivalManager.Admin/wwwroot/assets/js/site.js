// Function to toggle the dropdown menu
function ToggleDropdown() {
    var dropdown = document.getElementById("dropdownMenu");
    dropdown.classList.toggle("open");

    // Add an event listener to close the dropdown if clicking outside of it
    document.addEventListener('click', handleOutsideClick);
}

// Function to close the dropdown when pressing the Escape key
function CloseDropdownWithEscape(event) {
    if (event.key === "Escape") {
        CloseDropdown();
    }
}

// Function to close the dropdown
function CloseDropdown() {
    var dropdown = document.getElementById("dropdownMenu");
    dropdown.classList.remove("open");

    // Remove the event listener once dropdown is closed
    document.removeEventListener('click', handleOutsideClick);
}

// Function to handle clicks outside of the dropdown
function handleOutsideClick(event) {
    var dropdown = document.getElementById("dropdownMenu");

    // Check if the click is outside the dropdown
    if (!dropdown.contains(event.target)) {
        CloseDropdown();
    }
}