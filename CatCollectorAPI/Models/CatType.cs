namespace CatCollectorAPI.Models
{
    // Separate file that defines cat types
    public enum CatType
    {
        Good,
        Bad,
        Fat // frontend calls this "fat cat", backend stores as Chonky/Fat
    }
}
