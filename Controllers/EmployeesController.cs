using ASPNETMVCCURD.Data;
using ASPNETMVCCURD.Models;
using ASPNETMVCCURD.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ASPNETMVCCURD.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MVCDemoDbContext mvcDemoDbContext;
        public EmployeesController(IMemoryCache memoryCache, MVCDemoDbContext mvcDemoDbContext) 
        {
            this._memoryCache = memoryCache;
            this.mvcDemoDbContext = mvcDemoDbContext;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string SearchString) 
        {
            //if (string.IsNullOrEmpty(SearchString))
            //{
            //    var employees = await mvcDemoDbContext.Employees.ToListAsync();
            //    return View(employees);
            //}
            //else 
            //{
            //    //var employees = await mvcDemoDbContext.Employees.ToListAsync();
            //    var employees = await mvcDemoDbContext.Employees.Where(s => s.Name.Contains(SearchString)).ToListAsync();
            //    return View(employees);
            //}

            var cacheKey = "employeeList";
            //checks if cache entries exists
            if (!_memoryCache.TryGetValue(cacheKey, out List<Employee> employeeList))
            {
                if (string.IsNullOrEmpty(SearchString))
                {
                    //calling the server
                    employeeList = await mvcDemoDbContext.Employees.ToListAsync();
                    //setting up cache options
                    var cacheExpiryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddSeconds(50),
                        Priority = CacheItemPriority.High,
                        SlidingExpiration = TimeSpan.FromSeconds(20)
                    };
                    //setting cache entries
                    _memoryCache.Set(cacheKey, employeeList, cacheExpiryOptions);
                    return View(employeeList);
                }
            }
            else 
            {
                var employees = await mvcDemoDbContext.Employees.Where(s => s.Name.Contains(SearchString)).ToListAsync();
                return View(employees);
            }
            return View(employeeList);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddEmployeeViewModel addEmployeeRequest)
        {
            var employee = new Employee()
            {
                //Id = default(int),
                Name = addEmployeeRequest.Name,
                Email = addEmployeeRequest.Email,
                Salary = addEmployeeRequest.Salary,
                DateOfBirth = addEmployeeRequest.DateOfBirth,
                Department = addEmployeeRequest.Department

            };
            await mvcDemoDbContext.Employees.AddAsync(employee);
            await mvcDemoDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> View(int Id)
        {
            var employee = await mvcDemoDbContext.Employees.FirstOrDefaultAsync(x => x.Id == Id);
            if (employee != null)
            {
                var viewModel = new UpdateEmployeeViewModel()
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    Salary = employee.Salary,
                    DateOfBirth = employee.DateOfBirth,
                    Department = employee.Department
                };
                return await Task.Run(()=> View("View",viewModel));
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> View(UpdateEmployeeViewModel model) 
        {
            var employee = await mvcDemoDbContext.Employees.FindAsync(model.Id);
            if (employee != null) 
            {
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Salary = model.Salary;
                employee.DateOfBirth = model.DateOfBirth;
                employee.Department = model.Department;

                await mvcDemoDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(UpdateEmployeeViewModel model)
        {
            var employee = await mvcDemoDbContext.Employees.FindAsync(model.Id);
            if (employee != null)
            {
                mvcDemoDbContext.Employees.Remove(employee);

                await mvcDemoDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

    }
}
