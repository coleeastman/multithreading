using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 2.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/

namespace ConsoleApp1
{
    //delegate declaration for creating events
    public delegate void PriceCutEvent(double roomPrice, Thread agentThread);
    public delegate void OrderProcessEvent(Order order, double orderAmount);
    public delegate void OrderCreationEvent();


    public class MainClass
    {
        public static MultiCellBuffer buffer;
        public static Thread[] travelAgentThreads;
        public static bool hotelThreadRunning = true;
        public static void Main(string[] args)
        {
            
            Console.WriteLine("Inside Main");
            buffer = new MultiCellBuffer();

            Hotel hotel = new Hotel();
            TravelAgent travelAgent = new TravelAgent();

            Thread hotelThread = new Thread(new ThreadStart(hotel.hotelFun));
            hotelThread.Start();

            Hotel.PriceCut += new PriceCutEvent(travelAgent.agentOrder);
            Console.WriteLine("Price cut event has been subscribed");
            TravelAgent.orderCreation += new OrderCreationEvent(hotel.takeOrder);
            Console.WriteLine("Order creation event has been subscribed");
            OrderProcessing.OrderProcess += new OrderProcessEvent(travelAgent.orderProcessConfirm);
            Console.WriteLine("Order process event has been subscribed");

            travelAgentThreads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Creating  travel agent thread {0}", (i + 1));
                travelAgentThreads[i] = new Thread(travelAgent.agentFun);
                travelAgentThreads[i].Name = (i + 1).ToString();
                travelAgentThreads[i].Start();
            }
        }
    }


    public class MultiCellBuffer
    {
        // Each cell can contain an order object
        private const int bufferSize = 3; //buffer size
        int usedCells;
        private Order[] multiCells; // ? mark make the type nullable: allow to assign null value
        public static Semaphore getSemaph;
        public static Semaphore setSemaph;

        public MultiCellBuffer() //constructor 
        {
            // add your implementation here
            multiCells = new Order[bufferSize];
            usedCells = 0;
            getSemaph = new Semaphore(0, bufferSize);
            setSemaph = new Semaphore(bufferSize, bufferSize);
        }

        public void SetOneCell(Order data)
        {
            // add your implementation here

            // wait until there is an available cell to write to
            setSemaph.WaitOne();

            // Critical section: place the order in the buffer
            lock (this)
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    if (multiCells[i] == null)  // find the first empty spot in the buffer
                    {
                        Console.WriteLine("Setting in buffer cell");
                        multiCells[i] = data;  // place the order
                        usedCells++;  // increment the number of used cells
                        break;
                    }
                }
            }
            Console.WriteLine("Exit setting in buffer");

            // there is an order available to be read
            getSemaph.Release();

        }

        public Order GetOneCell()
        {
            // add your implementation here

            // wait until there is an order available to read
            getSemaph.WaitOne();

            Order retrievedOrder = null;

            lock (this)
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    if (multiCells[i] != null)  // find the first filled spot in the buffer
                    {
                        retrievedOrder = multiCells[i]; 
                        multiCells[i] = null;  // set cell to null (empty)
                        usedCells--;
                        Console.WriteLine("Exit reading buffer");
                        break;
                    }
                }
            }

            // Signal that there is now a free cell available for writing
            setSemaph.Release();

            return retrievedOrder;  // Return the retrieved order
        }
    }


    public class Order
    {
        //identity of sender of order
        private string senderId;
        //credit card number
        private long cardNo;
        //unit price of room from hotel
        private double unitPrice;
        //quantity of rooms to order
        private int quantity;

        //parametrized constructor
        public Order(string senderId, long cardNo, double unitPrice, int quantity)
        {
            // add your implementation here
            this.senderId = senderId;
            this.cardNo = cardNo;
            this.unitPrice = unitPrice;
            this.quantity = quantity;
        }

        //getter methods
        public string getSenderId()
        {
            // add your implementation here
            return senderId;
        }

        public long getCardNo()
        {
            // add your implementation here
            return cardNo;
        }
        public double getUnitPrice()
        {
            // add your implementation here
            return unitPrice;
        }
        public int getQuantity()
        {
            // add your implementation here
            return quantity;
        }

    }


    public class OrderProcessing
    {
        public static event OrderProcessEvent OrderProcess;
        //method to check for valid credit card number input

        public static bool creditCardCheck(long creditCardNumber)
        {
            // add your implementation here
            return creditCardNumber >= 5000 && creditCardNumber <= 7000;
        }

        // method to calculate the final charge after adding taxes, location charges, etc
        public static double calculateCharge(double unitPrice, int quantity)
        {
            // add your implementation here

            Random random = new Random();

            double taxRate = 0.08 + random.NextDouble() * (0.12 - 0.08);
            double locationCharge = random.Next(20, 81);

            double totalCharge = (unitPrice * quantity) * (1 + taxRate) + locationCharge;

            return totalCharge;
        }

        // method to process the order
        public static void ProcessOrder(Order order)
        {
            //add your implementation here
            if (creditCardCheck(order.getCardNo()))
            {
                double totalCharge = calculateCharge(order.getUnitPrice(), order.getQuantity());
                OrderProcess?.Invoke(order, totalCharge);  // Assuming you want to notify when an order is processed
                Console.WriteLine($"Travel Agent {order.getSenderId()}'s order is confirmed. The amount to be charged is ${totalCharge:F2}");
            }
            
        }


        
    }


    public class TravelAgent
    {
        public static event OrderCreationEvent orderCreation;

        public void agentFun()
        {
            // add your implementation here
            
            Console.WriteLine("Starting travel agent now");

            // bool priceCutOccurred = false;

            // // subscribe
            // Hotel.PriceCut += (roomPrice, thread) =>
            // {
            //     priceCutOccurred = true;
            // };

            // while (MainClass.hotelThreadRunning)
            // {
            //     // simulate checking for new prices
            //     Thread.Sleep(Random.Next(250, 501));

            //     if (priceCutOccurred)
            //     {
            //         if (Random.NextDouble() < 0.5) // 50% chance to try to create an order
            //         {
            //             createOrder(this.Name);
            //             priceCutOccurred = false; // reset flag after creating an order
            //         }
            //     }
            // }
        }
        public void orderProcessConfirm(Order order, double orderAmount)
        {
            // add your implementation here
            // Console.WriteLine($"Travel Agent {}'s order is confirmed. The amount to be charged is ${}");
        }

        private void createOrder(string senderId)
        {
            // add your implementation here

        }
        public void agentOrder(double roomPrice, Thread travelAgent) // Callback from hotel thread
        {
            // add your implementation here
        }
    }


    public class Hotel
    {
        static double currentRoomPrice = 100; //random current agent price
        static int threadNo = 0;
        static int eventCount = 0;
        public static event PriceCutEvent PriceCut;

        public void hotelFun()
        {
            // add your implementation here
            while (true)
            {
                pricingModel();

                Thread.Sleep(1000); // 1 sec
            }

        }
        //using random method to generate random room prices
        public double pricingModel()
        {
            Random random = new Random();

            double newPrice = random.Next(80, 161);

            Console.WriteLine($"New Price is {newPrice}");

            updatePrice(newPrice);

            return newPrice;

        }

        public void updatePrice(double newRoomPrice)
        {
            // add your implementation here
            if (newRoomPrice < currentRoomPrice)
            {
                Console.WriteLine("Updating the price and calling price cut event");
                PriceCut?.Invoke(newRoomPrice, Thread.CurrentThread);
            }
            currentRoomPrice = newRoomPrice;

        }

        public void takeOrder() // callback from travel agent
        {
            // add your implementation here
            while (true)
            {
                Order order = MainClass.buffer.GetOneCell();

                if (order != null)
                {
                    Console.WriteLine($"Incoming order for room with price {order.getUnitPrice()}");

                    OrderProcessing.ProcessOrder(order);
                }

                Thread.Sleep(1000); // 1 sec
            }

        }
    }
}


