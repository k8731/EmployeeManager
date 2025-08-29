using Microsoft.AspNetCore.Mvc;
using EmployeeManager.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NLog;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace EmployeeManager.Controllers
{

    public class EmployeeController : Controller
    {
        private readonly EmployeeContext _context;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(EmployeeContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 搜尋+分頁
        public IActionResult Index(string sortOrder, string searchName,  string searchDept, int page = 1)
        {
            _logger.LogInformation($"查詢員工清單 - name:{searchName}, dept:{searchDept}, sort:{sortOrder}, page:{page}");

            int pageSize = 5; // 每頁顯示5筆
            // 加上 AsNoTracking()，資料只讀，不追蹤
            var query = _context.Employees.AsNoTracking().AsQueryable();

            // 篩選姓名: 只顯示名字包含搜尋字串的員工，避免全表查詢
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(e => e.Name.Contains(searchName));

            // 篩選部門
            if (!string.IsNullOrEmpty(searchDept))
                query = query.Where(e => e.Department.Contains(searchDept));

            // 排序
            switch (sortOrder)
            {
                case "name_asc": query = query.OrderBy(e => e.Name); break;
                case "name_desc": query = query.OrderByDescending(e => e.Name); break;
                case "dept_asc": query = query.OrderBy(e => e.Department); break;
                case "dept_desc": query = query.OrderByDescending(e => e.Department); break;
                default: query = query.OrderBy(e => e.Id); break;
            }


            // 計算總頁數
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 限制 page 在合理範圍內
            page = Math.Max(1, Math.Min(page, totalPages));

            // 分頁取資料
            var employees = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 分頁後: 資料本身(employees) + 分頁資訊(viewbag)
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchName = searchName;
            ViewBag.SearchDept = searchDept;
            ViewBag.SortOrder = sortOrder;

            return View(employees);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Employees.Add(emp);
                    _context.SaveChanges();
                    _logger.LogInformation($"新增員工：{emp.Name} ({emp.Department})");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "新增員工失敗");
                    ModelState.AddModelError("", "新增員工時發生錯誤，請稍後再試。");
                }
            }
            return View(emp);
        }

        public IActionResult Edit(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null)
            {
                TempData["Error"] = "找不到指定員工";
                return RedirectToAction("Index");
            }
            return View(emp);
        }

        // POST: 接收編輯後的資料
        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emp);
                    _context.SaveChanges();
                    _logger.LogInformation($"編輯員工：{emp.Name} ({emp.Department})");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "編輯員工失敗");
                    ModelState.AddModelError("", "編輯員工時發生錯誤，請稍後再試。");
                }
            }
            return View(emp);
        }

        // POST: 確認刪除
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id, int page = 1, string searchName = "", string searchDept = "", string sortOrder = "")
        {
            var emp = _context.Employees.Find(id);
            if (emp == null)
            {
                TempData["Error"] = "找不到指定員工";
                return RedirectToAction("Index");
            }
            
            try
            {
                _context.Employees.Remove(emp);
                _context.SaveChanges();
                _logger.LogWarning($"刪除員工：{emp.Name} ({emp.Department})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除員工失敗");
                ModelState.AddModelError("", "刪除員工時發生錯誤，請稍後再試。");
            }

            // 刪除後回到原本的頁面與條件
            return RedirectToAction("Index", new
            {
                page = page,
                searchName = searchName,
                searchDept = searchDept,
                sortOrder = sortOrder
            });

        }

        public async Task<IActionResult> ExportCsv()
        {
            try
            {
                var employees = await _context.Employees.AsNoTracking().ToListAsync();

                // 建立 CSV 內容
                var csv = new StringBuilder();
                csv.AppendLine("Id,Name,Department,Email"); // 標題列

                foreach (var emp in employees)
                {
                    csv.AppendLine($"{emp.Id},{emp.Name},{emp.Department},{emp.Email}");
                }

                // 轉成 byte[]
                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                // 回傳檔案
                return File(bytes, "text/csv", "EmployeeList.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出 CSV 失敗");
                return BadRequest("匯出失敗，請稍後再試。");
            }
        }
    }
}
