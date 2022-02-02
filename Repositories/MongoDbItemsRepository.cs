using Catalog.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace  Catalog.Respositories;

public class MongoDbItemsRepository : IItemsRepository
{
    private const string databaseName = "catalog";
    private const string collectionName = "items";
    private readonly IMongoCollection<Item> itemsCollection;
    public  MongoDbItemsRepository(IMongoClient mongoClient){
        IMongoDatabase database = mongoClient.GetDatabase(databaseName);
        itemsCollection = database.GetCollection<Item>(collectionName);

    }
    public void CreateItem(Item item)
    {
        itemsCollection.InsertOne(item);
    }

    public void DeleteItem(Guid id)
    {
        //itemsCollection.DeleteOne();
    }

    public Item GetItem(Guid id)
    {
        
    }

    public IEnumerable<Item> GetItems()
    {
        return itemsCollection.Find(new BsonDocument()).ToList();
    }

    public void UpdateItem(Item item)
    {
        throw new NotImplementedException();
    }
}
