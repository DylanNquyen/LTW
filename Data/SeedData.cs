using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NguyenNgocThien_Buoi4.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NguyenNgocThien_BanHang.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Products.Any())
                {
                    return; // DB đã được seed
                }

                // Tạo các role
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roleNames = { "Admin", "Employee", "Customer" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@dylanshop.com",
                    Email = "admin@dylanshop.com",
                    FullName = "Admin",
                    Age = 30,
                    Address = "123 Admin Street",
                    PhoneNumber = "0123456789",
                    EmailConfirmed = true
                };

                var userExists = await userManager.FindByEmailAsync(adminUser.Email);
                if (userExists == null)
                {
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Employee user
                var employeeUser = new ApplicationUser
                {
                    UserName = "employee@dylanshop.com",
                    Email = "employee@dylanshop.com",
                    FullName = "Employee User",
                    Age = 25,
                    Address = "456 Employee Street",
                    PhoneNumber = "0987654321",
                    EmailConfirmed = true
                };

                userExists = await userManager.FindByEmailAsync(employeeUser.Email);
                if (userExists == null)
                {
                    var result = await userManager.CreateAsync(employeeUser, "Employee@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(employeeUser, "Employee");
                    }
                }

                // Customer user
                var customerUser = new ApplicationUser
                {
                    UserName = "customer@example.com",
                    Email = "customer@example.com",
                    FullName = "Customer User",
                    Age = 28,
                    Address = "789 Customer Street",
                    PhoneNumber = "0369852147",
                    EmailConfirmed = true
                };

                userExists = await userManager.FindByEmailAsync(customerUser.Email);
                if (userExists == null)
                {
                    var result = await userManager.CreateAsync(customerUser, "Customer@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(customerUser, "Customer");
                    }
                }

                // Danh mục
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Nhà bếp", Description = "Các sản phẩm dùng trong nhà bếp" },
                        new Category { Name = "Phòng ngủ", Description = "Các sản phẩm dùng trong phòng ngủ" },
                        new Category { Name = "Đồ điện tử", Description = "Các thiết bị điện tử gia dụng" },
                        new Category { Name = "Trang trí", Description = "Các sản phẩm trang trí nhà cửa" },
                        new Category { Name = "Phòng tắm", Description = "Các sản phẩm dùng trong phòng tắm" },
                        new Category { Name = "Văn phòng phẩm", Description = "Các sản phẩm văn phòng" }
                    );
                    await context.SaveChangesAsync();
                }

                // Sản phẩm
                if (!context.Products.Any())
                {
                    var categories = await context.Categories.ToListAsync();

                    var kitchenCategory = categories.FirstOrDefault(c => c.Name == "Nhà bếp");
                    var bedroomCategory = categories.FirstOrDefault(c => c.Name == "Phòng ngủ");
                    var electronicsCategory = categories.FirstOrDefault(c => c.Name == "Đồ điện tử");
                    var decorCategory = categories.FirstOrDefault(c => c.Name == "Trang trí");

                    // Kiểm tra tránh null
                    if (kitchenCategory == null || bedroomCategory == null || electronicsCategory == null || decorCategory == null)
                        throw new Exception("Một số danh mục không được tìm thấy trong database.");

                    context.Products.AddRange(
                        new Product
                        {
                            Name = "Bình giữ nhiệt Dylan",
                            Description = "Bình giữ nhiệt cao cấp, giữ nóng lạnh lên đến 12 giờ",
                            Price = 290000,
                            Stock = 50,
                            ImageUrl = "/images/products/binh-giu-nhiet.jpg",
                            CategoryId = kitchenCategory.Id
                        },
                        new Product
                        {
                            Name = "Bộ chăn ga gối Dylan",
                            Description = "Bộ chăn ga gối cotton 100%, mềm mại và thoáng khí",
                            Price = 1200000,
                            Stock = 30,
                            ImageUrl = "/images/products/bo-chan-ga-goi.jpg",
                            CategoryId = bedroomCategory.Id
                        },
                        new Product
                        {
                            Name = "Đèn ngủ thông minh",
                            Description = "Đèn ngủ thông minh điều khiển bằng giọng nói, nhiều chế độ ánh sáng",
                            Price = 450000,
                            Stock = 45,
                            ImageUrl = "/images/products/den-ngu-thong-minh.jpg",
                            CategoryId = electronicsCategory.Id
                        },
                        new Product
                        {
                            Name = "Nồi cơm điện Dylan",
                            Description = "Nồi cơm điện đa năng, nấu nhanh và tiết kiệm điện",
                            Price = 890000,
                            Stock = 25,
                            ImageUrl = "/images/products/noi-com-dien.jpg",
                            CategoryId = kitchenCategory.Id
                        },
                        new Product
                        {
                            Name = "Gối ôm Dylan",
                            Description = "Gối ôm cao cấp, êm ái và thoải mái",
                            Price = 350000,
                            Stock = 60,
                            ImageUrl = "/images/products/goi-om.jpg",
                            CategoryId = bedroomCategory.Id
                        },
                        new Product
                        {
                            Name = "Máy lọc không khí",
                            Description = "Máy lọc không khí hiện đại, lọc bụi mịn và khử mùi hiệu quả",
                            Price = 2500000,
                            Stock = 15,
                            ImageUrl = "/images/products/may-loc-khong-khi.jpg",
                            CategoryId = electronicsCategory.Id
                        },
                        new Product
                        {
                            Name = "Bộ dao nhà bếp Dylan",
                            Description = "Bộ dao nhà bếp 5 món, sắc bén và bền bỉ",
                            Price = 750000,
                            Stock = 40,
                            ImageUrl = "/images/products/bo-dao-nha-bep.jpg",
                            CategoryId = kitchenCategory.Id
                        },
                        new Product
                        {
                            Name = "Đồng hồ treo tường",
                            Description = "Đồng hồ treo tường thiết kế hiện đại, chạy êm",
                            Price = 420000,
                            Stock = 35,
                            ImageUrl = "/images/products/dong-ho-treo-tuong.jpg",
                            CategoryId = decorCategory.Id
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
