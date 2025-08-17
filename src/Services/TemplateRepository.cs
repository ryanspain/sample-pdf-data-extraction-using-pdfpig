using System.Text.Json;
using Blazored.LocalStorage;

public class TemplateRepository(ILocalStorageService LocalStorage)
{
    public async Task<Template[]> GetAll()
    {
        // TODO: Support multiple templates in the futures
        var item = await Get();
        
        if (item == null)
            return [];
        
        return [item];
    }
    
    public async Task<Template?> Get() 
    {
        var json = await LocalStorage.GetItemAsync<string>("template");

        if (json == null)
            return null;
        
        var template = JsonSerializer.Deserialize<Template>(json);

        return template ?? null;
    }

    public async Task<Template> Save(Template item)
    {
        var json = JsonSerializer.Serialize(item);
        
        await LocalStorage.SetItemAsync("template", json);
        
        return item;
    }

    public async Task Delete(int id) 
    {
        await LocalStorage.RemoveItemAsync("template");
    }
}