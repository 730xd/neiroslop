using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using rpm1920.Models;

namespace rpm1920
{
    public class ReportService
    {
        private readonly WarehouseDbContext _context;

        public ReportService()
        {
            _context = new WarehouseDbContext();
        }

        // ========== ОСНОВНЫЕ ОПЕРАЦИИ CRUD ==========

        public List<Movement> GetAllMovements()
        {
            return _context.Movements
                .OrderBy(m => m.Code)
                .ToList();
        }

        public void AddMovement(Movement movement)
        {
            _context.Movements.Add(movement);
            _context.SaveChanges();
        }

        public void UpdateMovement(Movement movement)
        {
            _context.Entry(movement).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteMovement(int code)
        {
            var movement = _context.Movements.Find(code);
            if (movement != null)
            {
                _context.Movements.Remove(movement);
                _context.SaveChanges();
            }
        }

        // ========== ДОБАВЛЕННЫЕ МЕТОДЫ ДЛЯ РАБОТЫ С ЦЕНАМИ ==========

        public Movement GetMovementByCode(int code)
        {
            return _context.Movements.FirstOrDefault(m => m.Code == code);
        }

        public PriceList GetPriceByCode(int code)
        {
            return _context.PriceLists.FirstOrDefault(p => p.Code == code);
        }

        public void UpdatePrice(PriceList price)
        {
            _context.Entry(price).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void AddPrice(PriceList price)
        {
            _context.PriceLists.Add(price);
            _context.SaveChanges();
        }

        // ========== УДАЛЕНИЕ СО ВСЕМИ СВЯЗЯМИ ==========

        public void DeleteDetailWithRelations(int code)
        {
            // Удаляем связанные поступления
            var receipts = _context.ReceiptInvoices.Where(r => r.Code == code);
            _context.ReceiptInvoices.RemoveRange(receipts);

            // Удаляем связанные выдачи
            var issues = _context.IssueRequests.Where(i => i.Code == code);
            _context.IssueRequests.RemoveRange(issues);

            // Удаляем цену
            var price = _context.PriceLists.FirstOrDefault(p => p.Code == code);
            if (price != null)
                _context.PriceLists.Remove(price);

            // Удаляем деталь
            var movement = _context.Movements.Find(code);
            if (movement != null)
                _context.Movements.Remove(movement);

            _context.SaveChanges();
        }

        // ========== ПОИСК (LINQ) ==========

        public List<DetailViewModel> SearchDetails(string searchText)
        {
            var query = from m in _context.Movements
                        join p in _context.PriceLists on m.Code equals p.Code into prices
                        from p in prices.DefaultIfEmpty()
                        where string.IsNullOrEmpty(searchText) || m.Name.Contains(searchText)
                        select new DetailViewModel
                        {
                            Code = m.Code,
                            Name = m.Name,
                            CurrentStock = (int)m.Quantity,
                            Price = p != null ? p.Price : 0,
                            TotalReceived = _context.ReceiptInvoices
                                .Where(r => r.Code == m.Code)
                                .Sum(r => (int?)r.QuantityReceived) ?? 0,
                            TotalIssued = _context.IssueRequests
                                .Where(i => i.Code == m.Code)
                                .Sum(i => (int?)i.QuantityIssued) ?? 0
                        };

            return query.ToList();
        }

        // ========== ЗАДАНИЕ 2: Детали за месяц (ТОЛЬКО LINQ) ==========

        public List<MonthlyReport> GetDetailsByMonth(int month, int year)
        {
            var query = from receipt in _context.ReceiptInvoices
                        join movement in _context.Movements
                            on receipt.Code equals movement.Code
                        where receipt.ReceiptDate.Month == month
                           && receipt.ReceiptDate.Year == year
                        group receipt by movement.Name into grouped
                        select new MonthlyReport
                        {
                            DetailName = grouped.Key,
                            TotalQuantity = grouped.Sum(x => x.QuantityReceived)
                        };

            return query.OrderBy(x => x.DetailName).ToList();
        }

        // ========== ЗАДАНИЕ 3: Стоимость по требованиям (ТОЛЬКО LINQ) ==========

        public List<RequestCostReport> GetCostByRequests()
        {
            var query = from issue in _context.IssueRequests
                        join price in _context.PriceLists
                            on issue.Code equals price.Code
                        group new { issue, price } by issue.RequestNumber into grouped
                        select new RequestCostReport
                        {
                            RequestNumber = grouped.Key,
                            TotalCost = grouped.Sum(x => x.issue.QuantityIssued * x.price.Price),
                            TotalItems = grouped.Count(),
                            TotalQuantity = grouped.Sum(x => x.issue.QuantityIssued)
                        };

            return query.OrderByDescending(x => x.TotalCost).ToList();
        }

        // ========== ДОПОЛНИТЕЛЬНЫЕ ОТЧЕТЫ (LINQ) ==========

        public List<ReceiptReport> GetAllReceipts()
        {
            var query = from receipt in _context.ReceiptInvoices
                        join movement in _context.Movements
                            on receipt.Code equals movement.Code
                        orderby receipt.ReceiptDate descending
                        select new ReceiptReport
                        {
                            InvoiceNumber = receipt.InvoiceNumber,
                            Code = receipt.Code,
                            DetailName = movement.Name,
                            QuantityReceived = receipt.QuantityReceived,
                            ReceiptDate = receipt.ReceiptDate
                        };

            return query.ToList();
        }

        public List<IssueReport> GetAllIssues()
        {
            var query = from issue in _context.IssueRequests
                        join movement in _context.Movements
                            on issue.Code equals movement.Code
                        orderby issue.IssueDate descending
                        select new IssueReport
                        {
                            RequestNumber = issue.RequestNumber,
                            Code = issue.Code,
                            DetailName = movement.Name,
                            QuantityIssued = issue.QuantityIssued,
                            IssueDate = issue.IssueDate
                        };

            return query.ToList();
        }

        public List<PriceReport> GetAllPrices()
        {
            var query = from price in _context.PriceLists
                        join movement in _context.Movements
                            on price.Code equals movement.Code
                        orderby movement.Name
                        select new PriceReport
                        {
                            Code = price.Code,
                            DetailName = movement.Name,
                            Price = price.Price
                        };

            return query.ToList();
        }

        public List<StockReport> GetLowStock(int threshold = 50)
        {
            var query = from movement in _context.Movements
                        where movement.Quantity < threshold
                        select new StockReport
                        {
                            Code = movement.Code,
                            Name = movement.Name,
                            CurrentStock = (int)movement.Quantity
                        };

            return query.ToList();
        }

        public List<StockBalanceReport> GetStockBalance()
        {
            var query = from m in _context.Movements
                        select new StockBalanceReport
                        {
                            DetailName = m.Name,
                            Balance = (int)m.Quantity
                        };

            return query.OrderBy(x => x.DetailName).ToList();
        }

        // ЗАДАНИЕ 2: Движение детали на определенную дату
        public List<MovementDetailReport> GetMovementByDate(DateTime date)
        {
            var receipts = from r in _context.ReceiptInvoices
                           where r.ReceiptDate.Date == date.Date
                           join m in _context.Movements on r.Code equals m.Code
                           select new MovementDetailReport
                           {
                               DetailName = m.Name,
                               ReceiptDate = r.ReceiptDate,
                               QuantityReceived = r.QuantityReceived,
                               IssueDate = null,
                               QuantityIssued = null
                           };

            var issues = from i in _context.IssueRequests
                         where i.IssueDate.Date == date.Date
                         join m in _context.Movements on i.Code equals m.Code
                         select new MovementDetailReport
                         {
                             DetailName = m.Name,
                             ReceiptDate = null,
                             QuantityReceived = null,
                             IssueDate = i.IssueDate,
                             QuantityIssued = i.QuantityIssued
                         };

            var result = receipts.Union(issues).OrderBy(x => x.DetailName).ToList();
            return result;
        }

        // ЗАДАНИЕ 3: Добавление данных в таблицу Справочник цен (уже есть AddPrice)
        // Используем существующий метод AddPrice

        // ЗАДАНИЕ 4: Стоимость деталей на складе
        public List<StockValueReport> GetStockValue()
        {
            var query = from m in _context.Movements
                        join p in _context.PriceLists on m.Code equals p.Code
                        select new StockValueReport
                        {
                            DetailName = m.Name,
                            Quantity = (int)m.Quantity,
                            Price = p.Price,
                            TotalValue = (decimal)m.Quantity * p.Price
                        };

            return query.OrderByDescending(x => x.TotalValue).ToList();
        }

        // ЗАДАНИЕ 5: Деталь, которая выдавалась чаще остальных
        public MostIssuedDetailReport GetMostIssuedDetail()
        {
            var query = from i in _context.IssueRequests
                        join m in _context.Movements on i.Code equals m.Code
                        group i by new { i.Code, m.Name } into g
                        select new
                        {
                            Code = g.Key.Code,
                            DetailName = g.Key.Name,
                            TotalIssued = g.Sum(x => x.QuantityIssued)
                        };

            var result = query.OrderByDescending(x => x.TotalIssued).FirstOrDefault();

            if (result != null)
            {
                return new MostIssuedDetailReport
                {
                    DetailName = result.DetailName,
                    TotalIssued = result.TotalIssued
                };
            }

            return null;
        }

        // ДОПОЛНИТЕЛЬНО: Полная стоимость всех деталей на складе
        public decimal GetTotalStockValue()
        {
            var query = from m in _context.Movements
                        join p in _context.PriceLists on m.Code equals p.Code
                        select m.Quantity * p.Price;

            return (decimal)query.Sum();
        }


        // ========== DTO КЛАССЫ ДЛЯ ОТЧЕТОВ ==========
        public class StockBalanceReport
        {
            public string DetailName { get; set; } = string.Empty;
            public int Balance { get; set; }
        }

        // Задание 2
        public class MovementDetailReport
        {
            public string DetailName { get; set; } = string.Empty;
            public DateTime? ReceiptDate { get; set; }
            public int? QuantityReceived { get; set; }
            public DateTime? IssueDate { get; set; }
            public int? QuantityIssued { get; set; }
        }

        // Задание 4
        public class StockValueReport
        {
            public string DetailName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalValue { get; set; }
        }

        // Задание 5
        public class MostIssuedDetailReport
        {
            public string DetailName { get; set; } = string.Empty;
            public int TotalIssued { get; set; }
        }
        public class DetailViewModel
        {
            public int Code { get; set; }
            public string Name { get; set; } = string.Empty;
            public int CurrentStock { get; set; }
            public decimal Price { get; set; }
            public int TotalReceived { get; set; }
            public int TotalIssued { get; set; }
            public int Balance => TotalReceived - TotalIssued;
        }

        public class MonthlyReport
        {
            public string DetailName { get; set; } = string.Empty;
            public int TotalQuantity { get; set; }
        }

        public class RequestCostReport
        {
            public string RequestNumber { get; set; } = string.Empty;
            public decimal TotalCost { get; set; }
            public int TotalItems { get; set; }
            public int TotalQuantity { get; set; }
        }

        public class ReceiptReport
        {
            public string InvoiceNumber { get; set; } = string.Empty;
            public int Code { get; set; }
            public string DetailName { get; set; } = string.Empty;
            public int QuantityReceived { get; set; }
            public DateTime ReceiptDate { get; set; }
        }

        public class IssueReport
        {
            public string RequestNumber { get; set; } = string.Empty;
            public int Code { get; set; }
            public string DetailName { get; set; } = string.Empty;
            public int QuantityIssued { get; set; }
            public DateTime IssueDate { get; set; }
        }

        public class PriceReport
        {
            public int Code { get; set; }
            public string DetailName { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }

        public class StockReport
        {
            public int Code { get; set; }
            public string Name { get; set; } = string.Empty;
            public int CurrentStock { get; set; }
        }
    }
}


