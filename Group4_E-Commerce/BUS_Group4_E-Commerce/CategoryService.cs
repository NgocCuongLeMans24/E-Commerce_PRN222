using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(EcommercePrn222Context context)
        {
            _categoryRepository = new CategoryRepository(context);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                // Check if category name already exists
                if (await CategoryExistsAsync(category.CategoryName))
                {
                    return false;
                }

                return await _categoryRepository.CreateAsync(category);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                // Check if category name already exists (excluding current category)
                if (await CategoryExistsAsync(category.CategoryName, category.CategoryId))
                {
                    return false;
                }

                return await _categoryRepository.UpdateAsync(category);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                // Check if category has products
                var productCount = await GetProductCountByCategoryAsync(id);
                if (productCount > 0)
                {
                    return false; // Cannot delete category with products
                }

                return await _categoryRepository.DeleteAsync(id);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CategoryExistsAsync(string categoryName, int? excludeId = null)
        {
            return await _categoryRepository.ExistsAsync(categoryName, excludeId);
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _categoryRepository.GetProductCountAsync(categoryId);
        }
    }
}
