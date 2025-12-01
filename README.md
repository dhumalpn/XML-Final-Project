# Inventory Management Web Application Design Document

## I. Introduction

This document outlines the design for the Inventory Management Web App. The application is a web-based tool that allows users to track their household inventory across various categories and locations. Users can add products (e.g., "Chobani Greek Yogurt"), specify their location (e.g., "Pantry"), and manage data like quantity and expiry dates.

A key feature of this application is the integration of external data sources to provide users with valuable insights. Once a product is added (especially food items), the app will fetch and display:

* Health Score: A rating based on the product's ingredients.
* Green Score: A rating indicating the product's environmental friendliness.


## II. Icon/Logo
![SafeStreet (1) (1)](https://github.com/dhumalpn/7024-Inventory-Management-Web-App/blob/main/InventoryManagement/InventoryManagement/wwwroot/img/Logo.png)



## III. Storyboard

The following describes the intended user flow and screen layout-

### Screen 1: Dashboard (Home Screen)

* **Purpose:** Provide a high-level overview and quick navigation.
* **Widgets:**
    * Header: App Logo and Title ("My Inventory").
    * "Quick Add" Button: A prominent "+" button to quickly add a new item.
    * "Alerts" Section (Future Enhancement):
        * "Expiring Soon" (e.g., "Eggs - 2 days left").
        * "Low Stock" (e.g., "Milk - 0 remaining").
    * "View by Place" List: A list of configured locations (e.g., "Pantry," "Restroom," "Office") with an item count for each.
    * Navigation Bar (Bottom): Home | Inventory | Shopping List (Future) | Settings.

### Screen 2: Inventory List (Filtered by Place)

* **Purpose:** Display all items within a selected location.
* **Trigger:** User taps on "Pantry" from the Dashboard.
* **Widgets:**
    * Header: "Pantry".
    * Search Bar: To filter items within the pantry.
    * Item List: A scrollable list of products. Each list item shows:
        * Product Image (small thumbnail).
        * Product Name ("Chobani Greek Yogurt").
        * Quantity ("Qty: 2").
        * Expiry Date ("Exp: 11/15/2025").

### Screen 3: Add/Edit Item Screen

* **Purpose:** Add a new item or edit an existing one.
* **Trigger:** User taps the "Quick Add" button or taps an existing item from the list.
* **Widgets:**
    * "Scan Barcode" Button: (Future enhancement).
    * “Add item” Button: (Manual Input method)
    * Product Image: (Populated from API or user upload).
    * Product Name: (e.g., "Chobani Greek Yogurt, Vanilla").
    * Quantity: (Input field, e.g., "2").
    * Expiry Date: (Date picker).
    * Place (Dropdown): "Pantry," "Restroom," etc.
    * Category (Dropdown): "Dairy," "Grains," etc. (Often populated automatically from API data).
    * Scores Section (Read-only):
        * Health Score: "Good" (75/100).
        * Green Score: "B" (Eco-Score).
    * "Save" Button.
    * "Delete" Button (Only visible when editing).


## IV. Projects (GitHub)

Work will be planned and tracked using GitHub Projects within the project repository.

Link - https://github.com/dhumalpn/7024-Inventory-Management-Web-App


## V. User Requirements

### Requirement 1: Adding a Product

As a user, I want to add a new product to my inventory so that I can track its quantity, location, and expiry.
* **Example 1.1 (Adding via API lookup):**
    * Given I am on the "Add Item" screen.
    * When I search for "Chobani Greek Yogurt Vanilla" and select the correct item from the results.
    * Then the app should query Open Food Facts, auto-fill the Product Name, Category ("Dairy"), Health Score, and Green Score.
* **Example 1.2 (Setting custom data):**
    * Given I have just looked up "Chobani Greek Yogurt."
    * When I set the Quantity to "4", select "Pantry" as the Place, and set the Expiry Date to "11/20/2025".
    * Then the item is saved to my "Pantry" inventory with all the specified details.
* **Example 1.3 (Adding a non-food item):**
    * Given I am on the "Add Item" screen.
    * When I manually type "Paper Towels", set Category to "Office" (or "Household"), set Place to "Restroom", and Quantity to "1".
    * Then the item is saved to my "Restroom" inventory (Health and Green Scores may be "N/A").

### Requirement 2: Viewing Product Scores

As a user, I want to see the Health Score and Green Score for my products so that I can make informed decisions about my items.
* **Example 2.1 (Viewing scores on details page):**
    * Given I have "Chobani Greek Yogurt" in my inventory.
    * When I tap on that item from the "Inventory List" to open the "Edit Item" screen.
    * Then I should see a "Health Score" (e.g., "Good") and a "Green Score" (e.g., "Eco-Score: B") displayed clearly on the screen.

### Requirement 3: Managing Inventory

As a user, I want to update the quantity and expiry date of an item so that my inventory remains accurate.
* **Example 3.1 (Updating quantity):**
    * Given I am viewing the "Item Details" for "Milk," which has a current quantity of "1".
    * When I change the quantity input field to "0" (because I finished it) and press "Save".
    * Then the inventory list should reflect that "Milk" has a quantity of "0".
* **Example 3.2 (Updating expiry date):**
    * Given I am viewing the "Item Details" for "Eggs" with an expiry date of "11/10/2025".
    * When I use the date picker to change the expiry date to "11/17/2025" and press "Save".
    * Then the item "Eggs" is saved with the new expiry date.

### Requirement 4: Organizing Inventory

As a user, I want to filter my inventory list by place so that I can easily see what I have in a specific location (e.g., "Pantry").
* **Example 4.1 (Filtering by place):**
    * Given I have "Cereal" saved to "Pantry" and "Shampoo" saved to "Restroom".
    * When I navigate to the Dashboard and tap on the "Pantry" location.
    * Then I should see "Cereal" in the resulting "Inventory List", but I should not see "Shampoo".


## VI. Data Sources

**External Data Sources (APIs):**

1.  **Open Food Facts API:**
    * URL: https://world.openfoodfacts.org
    * Usage: This will be the primary source for product data. We will use it to fetch:
        * Product Name & Image.
        * Ingredients list (to determine Health Score).
        * Eco-Score (to be used as our "Green Score").
        * Nutri-Score (to be used as our "Health Score").
        * Product categorization (which we can map to our internal categories).
2.  **UPC Item DB API (or similar):**
    * URL: https://www.upcitemdb.com/api/
    * Usage: A secondary lookup service. If a product isn't found in Open Food Facts, or for non-food items, this API can be used to look up a product name from a scanned barcode (UPC).

**Internally Developed Data Sources:**

1.  **Place Categorization (XML/JSON):**
    * Format: A simple XML file stored within the application (e.g., places.xml). This file will be read by the app to populate the "Place" dropdown.
    * Example Data: Pantry, Restroom, Bedroom, Office, Frozen.
2.  **Product Categorization (XML/JSON):**
    * Format: A simple XML file (e.g., categories.xml) that defines the parent categories we want to use.
    * Usage: This will populate the "Category" dropdown. We will attempt to map the detailed categories from Open Food Facts (e.g., "yogurts") to our simpler internal categories (e.g., "Dairy").
    * Example Data: Fruits, Vegetables, Grains, Dairy, Bakery, Frozen, Household.

**User-Inputted Data (Stored in Database):**

* User Inventory: The core database table linking a UserID to a ProductID.
* Quantity: The current count of the item, entered by the user.
* Expiry Date: (Optional) The specific expiry date for the item, set by the user.
* Custom Image: (Optional) A photo uploaded by the user if an API image is not available or incorrect.


## VII. Team Composition

   ### Team Members
   (Responsibilities of each member to be determined in the next meeting)
   * Harshitha Baratam
   * Narayan Shanku
   * Pooja Dhumal
   * Rohan Barai

   ### Weekly Team Meeting

   Every Thursday, 8:00 PM - 09:00 PM at Lindner College of Business
