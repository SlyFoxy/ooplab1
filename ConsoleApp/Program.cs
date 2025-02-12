using System;

// Абстрактний клас для страви
abstract class Dish
{
    public abstract string Prepare();
}

// Конкретні страви
class Pizza : Dish
{
    public override string Prepare()
    {
        return "Готується піца з інгредієнтами.";
    }
}

class Pasta : Dish
{
    public override string Prepare()
    {
        return "Готується паста з соусом.";
    }
}

// Фабричний метод
abstract class DishFactory
{
    public abstract Dish CreateDish();
}

// Конкретні фабрики
class PizzaFactory : DishFactory
{
    public override Dish CreateDish()
    {
        return new Pizza();
    }
}

class PastaFactory : DishFactory
{
    public override Dish CreateDish()
    {
        return new Pasta();
    }
}

// Клієнтський код
class Program
{
    static void Main()
    {
        DishFactory pizzaFactory = new PizzaFactory();
        DishFactory pastaFactory = new PastaFactory();
        
        Console.WriteLine(OrderDish(pizzaFactory));  // Готується піца з інгредієнтами.
        Console.WriteLine(OrderDish(pastaFactory));  // Готується паста з соусом.
    }
    
    static string OrderDish(DishFactory factory)
    {
        Dish dish = factory.CreateDish();
        return dish.Prepare();
    }
}
