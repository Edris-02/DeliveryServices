# Order Items Status Implementation

## Overview
This implementation adds functionality to track individual order item statuses, allowing some items in an order to be delivered while others may be pending or cancelled.

## Changes Made

### 1. Model Updates

#### OrderItems.cs
- Added `OrderItemStatus` enum with three states:
  - `Pending` - Default state for new items
  - `Delivered` - Item has been delivered to customer
  - `Cancelled` - Item was cancelled by customer
- Added `Status` property to `OrderItems` model (defaults to `Pending`)

#### Orders.cs
- Updated `ComputeSubTotal()` method to **only include delivered items** in the subtotal calculation
- Added computed properties:
  - `DeliveredItemsCount` - Count of items with Delivered status
  - `CancelledItemsCount` - Count of items with Cancelled status
  - `PendingItemsCount` - Count of items with Pending status

### 2. Controller Updates (OrdersController.cs)

#### New Action Method
- **`UpdateItemStatus(int orderId, int itemId, OrderItemStatus status)`**
  - Updates the status of individual order items
  - Automatically adjusts merchant balance when items are marked as delivered or changed from delivered
  - Only delivered items affect merchant balance

#### Modified Action Methods
- **`AddItem()`**
  - New items default to `Pending` status
  - No merchant balance update until item is marked as delivered

- **`UpdateItem()`**
  - Only includes delivered items in merchant balance calculations
  - Merchant balance is only affected if the item status is `Delivered`

- **`RemoveItem()`**
  - Only adjusts merchant balance if the removed item was in `Delivered` status

### 3. View Updates (Details.cshtml)

#### Order Items Table
- Added **Status column** showing current status with color-coded badges:
  - ?? Yellow (Warning) - Pending
  - ?? Green (Success) - Delivered
  - ?? Red (Danger) - Cancelled
- Cancelled items are displayed with strikethrough text
- Added **Update Status button** for each item

#### Item Status Modal
- New modal dialog to update individual item status
- Shows current status and allows selection of new status
- Displays informational alert explaining that only delivered items are included in totals

#### Footer Statistics
- Shows breakdown of items: Total, Delivered, Pending, Cancelled
- Clarifies that "Subtotal" represents only delivered items
- Merchant amount now explicitly shows "(Delivered Items Only)"

#### Quick Stats Panel
- Replaced simple item count with detailed statistics:
  - Total items count
  - Delivered items count (green badge)
  - Pending items count (yellow badge)
  - Cancelled items count (red badge)

## Business Logic

### Merchant Balance Calculation
- **Only delivered items** contribute to merchant balance
- When an item status changes:
  - `Pending ? Delivered`: Add item value to merchant balance (if order is delivered)
  - `Delivered ? Pending/Cancelled`: Subtract item value from merchant balance (if order is delivered)
  - `Cancelled ? Delivered`: Add item value to merchant balance (if order is delivered)

### Order Total Calculation
- **SubTotal**: Sum of (Quantity × UnitPrice) for delivered items only
- **Total**: SubTotal + DeliveryFee
- **Merchant Amount**: SubTotal (delivered items only, excluding delivery fee)

## Database Migration Required

To apply these changes to the database, you need to create and run a migration:

```bash
# Create migration
dotnet ef migrations add AddOrderItemStatus --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web

# Update database
dotnet ef database update --project DeliveryServices.DataAccess --startup-project DeliveryServices.Web
```

## Usage Workflow

1. **Create Order**: Add order with items (all items start as Pending)
2. **Add/Edit Items**: Manage order items as needed
3. **Update Item Status**: 
   - Mark items as `Delivered` when successfully delivered
   - Mark items as `Cancelled` if customer cancels specific items
   - Keep items as `Pending` while preparing/processing
4. **Monitor**: Use the statistics panel to track item statuses
5. **Financial Tracking**: Only delivered items affect merchant balance and order subtotal

## Benefits

? **Granular Control**: Track status of each item individually
? **Partial Deliveries**: Support orders where some items are delivered and others aren't
? **Accurate Accounting**: Merchant balance only reflects delivered items
? **Better Visibility**: Clear visual indicators of item status
? **Flexible Management**: Handle customer cancellations of specific items

## Example Scenario

An order has 3 items:
- Item A: Pizza ($15) - Status: Delivered ?
- Item B: Burger ($10) - Status: Delivered ?
- Item C: Fries ($5) - Status: Cancelled ?

**Result:**
- Subtotal: $25 (only delivered items)
- Delivery Fee: $5
- Total: $30
- Merchant Amount: $25
