using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Collections.Generic;

namespace DurableFunction
{
    public class OrderFunction
    {
        private readonly YourDbContext _context;

        public OrderFunction(YourDbContext context)
        {
            _context = context;
        }

        [Function(nameof(OrderOrchestrator))]
        public static async Task OrderOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var orderId = context.GetInput<int>();

            // Step 1: Update Inventory
            await context.CallActivityAsync(nameof(UpdateInventory), orderId);

            // Step 2: Process Payment
            await context.CallActivityAsync(nameof(ProcessPayment), orderId);

            // Step 3: Confirm Order
            await context.CallActivityAsync(nameof(ConfirmOrder), orderId);

            // Step 4: Notify User
            await context.CallSubOrchestratorAsync(nameof(NotificationOrchestrator), orderId);
        }

        [Function(nameof(UpdateInventory))]
        public async Task UpdateInventory([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateInventory");
            logger.LogInformation($"Updating inventory for order {orderId}.");

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    logger.LogWarning($"Order {orderId} not found.");
                    return;
                }

                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= detail.Quantity;
                        if (product.StockQuantity < 0)
                        {
                            product.StockQuantity = 0;
                            logger.LogWarning($"Stock quantity for product {product.ProductId} went negative, setting to 0.");
                        }
                        _context.Products.Update(product);
                    }
                    else
                    {
                        logger.LogWarning($"Product {detail.ProductId} not found.");
                    }
                }

                await _context.SaveChangesAsync();
                logger.LogInformation($"Inventory update for order {orderId} completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while updating inventory for order {orderId}: {ex.Message}");
            }
        }

        [Function(nameof(ProcessPayment))]
        public async Task ProcessPayment([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ProcessPayment");
            logger.LogInformation($"Processing payment for order {orderId}.");

            try
            {
                // Implement your payment processing logic here
                await Task.Delay(500); // Simulate some processing time
                logger.LogInformation($"Payment processed for order {orderId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while processing payment for order {orderId}: {ex.Message}");
            }
        }

        [Function(nameof(ConfirmOrder))]
        public async Task ConfirmOrder([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ConfirmOrder");
            logger.LogInformation($"Confirming order {orderId}.");

            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    logger.LogWarning($"Order {orderId} not found.");
                    return;
                }

                order.Status = "Confirmed";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                logger.LogInformation($"Order {orderId} confirmed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while confirming order {orderId}: {ex.Message}");
            }
        }

        [Function(nameof(NotificationOrchestrator))]
        public static async Task NotificationOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var orderId = context.GetInput<int>();

            // Step 1: Send Order Received Notification
            await context.CallActivityAsync(nameof(SendOrderReceivedNotification), orderId);

            // Step 2: Send Payment Processed Notification
            await context.CallActivityAsync(nameof(SendPaymentProcessedNotification), orderId);

            // Step 3: Send Order Confirmed Notification
            await context.CallActivityAsync(nameof(SendOrderConfirmedNotification), orderId);
        }

        [Function(nameof(SendOrderReceivedNotification))]
        public async Task SendOrderReceivedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendOrderReceivedNotification");
            logger.LogInformation($"Sending order received notification for order {orderId}.");

            try
            {
                // Implement your notification logic here
                await Task.Delay(500); // Simulate some processing time
                logger.LogInformation($"Order received notification sent for order {orderId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while sending order received notification for order {orderId}: {ex.Message}");
            }
        }

        [Function(nameof(SendPaymentProcessedNotification))]
        public async Task SendPaymentProcessedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendPaymentProcessedNotification");
            logger.LogInformation($"Sending payment processed notification for order {orderId}.");

            try
            {
                // Implement your notification logic here
                await Task.Delay(500); // Simulate some processing time
                logger.LogInformation($"Payment processed notification sent for order {orderId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while sending payment processed notification for order {orderId}: {ex.Message}");
            }
        }

        [Function(nameof(SendOrderConfirmedNotification))]
        public async Task SendOrderConfirmedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendOrderConfirmedNotification");
            logger.LogInformation($"Sending order confirmed notification for order {orderId}.");

            try
            {
                // Implement your notification logic here
                await Task.Delay(500); // Simulate some processing time
                logger.LogInformation($"Order confirmed notification sent for order {orderId}.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while sending order confirmed notification for order {orderId}: {ex.Message}");
            }
        }

        [Function("OrderOrchestrator_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("OrderOrchestrator_HttpStart");
            var requestBody = await req.ReadAsStringAsync();
            var orderId = int.Parse(requestBody);

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrderOrchestrator), orderId);

            logger.LogInformation($"Started order orchestration with ID = '{instanceId}' for order ID = '{orderId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
}

public class Order
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
}

public class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public Order Order { get; set; }
}

public class Product
{
    public int ProductId { get; set; }
    public int StockQuantity { get; set; }
}
