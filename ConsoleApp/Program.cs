using System;
using System.Collections.Generic;

//----------------------------LAB 1-----------------------------------------------
// Фабричний метод та Абстрактна фабрика
abstract class Dish
{
    public abstract string Prepare();
}

class Pizza : Dish
{
    public override string Prepare() => "Готується піца з інгредієнтами.";
}

class Pasta : Dish
{
    public override string Prepare() => "Готується паста з соусом.";
}

abstract class DishFactory
{
    public abstract Dish CreateDish();
}

class PizzaFactory : DishFactory
{
    public override Dish CreateDish() => new Pizza();
}

class PastaFactory : DishFactory
{
    public override Dish CreateDish() => new Pasta();
}

//----------------------------LAB 2-----------------------------------------------
// Прототип
public interface ICloneableOrder
{
    Order Clone();
}

// Будівельник
public interface IOrderBuilder
{
    void AddDish(string dish);
    Order Build();
}

public class OrderBuilder : IOrderBuilder
{
    private Order _order = new Order();

    public void AddDish(string dish) => _order.AddDish(dish);

    public Order Build() => _order;
}

public class Director
{
    public Order CreateComboMeal(IOrderBuilder builder)
    {
        builder.AddDish("Burger");
        builder.AddDish("Fries");
        builder.AddDish("Cola");
        return builder.Build();
    }
}

//----------------------------LAB 3-----------------------------------------------
// Стратегія
public interface IPricingStrategy
{
    decimal CalculatePrice(Order order);
}

public class RegularPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice;
}

public class DiscountPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice * 0.9m;
}

public class PremiumPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice * 1.2m;
}

// Спостерігач
public interface IOrderObserver
{
    void Update(Order order);
}

public class Waiter : IOrderObserver
{
    public void Update(Order order) => Console.WriteLine("Офіціант отримав оновлення: Нове замовлення!");
}

public class Chef : IOrderObserver
{
    public void Update(Order order) => Console.WriteLine("Шеф-кухар отримав оновлення: Нове замовлення на кухні!");
}

public class OrderNotifier
{
    private List<IOrderObserver> _observers = new List<IOrderObserver>();

    public void Attach(IOrderObserver observer) => _observers.Add(observer);
    public void Detach(IOrderObserver observer) => _observers.Remove(observer);
    public void Notify(Order order)
    {
        foreach (var observer in _observers)
        {
            observer.Update(order);
        }
    }
}

// Команда
public interface ICommand
{
    void Execute();
    void Undo();
}

public class AddDishCommand : ICommand
{
    private Order _order;
    private string _dish;

    public AddDishCommand(Order order, string dish)
    {
        _order = order;
        _dish = dish;
    }

    public void Execute()
    {
        _order.AddDish(_dish);
        Console.WriteLine($"Страва {_dish} додана до замовлення.");
    }

    public void Undo()
    {
        _order.RemoveDish(_dish);
        Console.WriteLine($"Страва {_dish} видалена з замовлення.");
    }
}

public class Order : ICloneableOrder
{
    public decimal BasePrice { get; private set; } = 100;
    public List<string> Dishes { get; private set; } = new List<string>();
    public IPricingStrategy PricingStrategy { get; set; }
    public OrderNotifier Notifier { get; private set; } = new OrderNotifier();

    public void AddDish(string dish)
    {
        Dishes.Add(dish);
        Notifier.Notify(this);
    }

    public void RemoveDish(string dish)
    {
        Dishes.Remove(dish);
        Notifier.Notify(this);
    }

    public decimal GetFinalPrice() => PricingStrategy?.CalculatePrice(this) ?? BasePrice;

    public Order Clone()
    {
        Order copy = new Order { PricingStrategy = this.PricingStrategy };
        copy.Dishes.AddRange(this.Dishes);
        return copy;
    }

    public void ShowOrder()
    {
        Console.WriteLine("Order contains: " + string.Join(", ", Dishes));
    }
}


class Program
{
    static void Main()
    {
        //------------------------LAB 1-----------------------------------
        DishFactory pizzaFactory = new PizzaFactory();
        DishFactory pastaFactory = new PastaFactory();
        
        Console.WriteLine(OrderDish(pizzaFactory));
        Console.WriteLine(OrderDish(pastaFactory));

        //----------------------LAB 2-------------------------------------
        Console.WriteLine("--- Prototype Pattern ---");
        Order originalOrder = new Order();
        originalOrder.AddDish("Pizza");
        originalOrder.AddDish("Pasta");

        Order clonedOrder = originalOrder.Clone();
        clonedOrder.ShowOrder();

        Console.WriteLine("--- Builder Pattern ---");
        IOrderBuilder builder = new OrderBuilder();
        builder.AddDish("Steak");
        builder.AddDish("Salad");

        Order customOrder = builder.Build();
        customOrder.ShowOrder();

        Director director = new Director();
        Order comboMeal = director.CreateComboMeal(new OrderBuilder());
        comboMeal.ShowOrder();

        //----------------------LAB 3-------------------------------------
        Order order = new Order();
        order.PricingStrategy = new DiscountPricing();

        Waiter waiter = new Waiter();
        Chef chef = new Chef();
        order.Notifier.Attach(waiter);
        order.Notifier.Attach(chef);

        ICommand addDish = new AddDishCommand(order, "Піца");
        addDish.Execute();
        Console.WriteLine($"Фінальна ціна: {order.GetFinalPrice()} грн");
        addDish.Undo();
    }

    //--------------------LAB 1------------------------------
    static string OrderDish(DishFactory factory)
    {
        Dish dish = factory.CreateDish();
        return dish.Prepare();
    }
}
