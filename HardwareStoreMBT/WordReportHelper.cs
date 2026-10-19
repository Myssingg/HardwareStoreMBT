using System;
using System.Data.Entity;
using System.Linq;
using Word = Microsoft.Office.Interop.Word;
using System.IO;

namespace HardwareStoreMBT
{
    public static class WordReportHelper
    {
        public static void PrintOrderCheck(int orderId)
        {
            var app = new Word.Application { Visible = true };
            var doc = app.Documents.Add();
            var r = doc.Range();

            using (var db = new HardwareStoreMBTEntities())
            {
                var order = db.orders.Include(o => o.clients).Include(o => o.orderStatuses).Include(o => o.paymentMethods).FirstOrDefault(o => o.id == orderId);
                if (order == null) throw new Exception("Заказ не найден");

                var items = db.productsInOrder.Include(p => p.products).Where(p => p.orderId == orderId).ToList();

                r.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                r.Font.Size = 16; r.Font.Bold = 1; r.Text = "ТОВАРНЫЙ ЧЕК\n\n";

                var info = doc.Paragraphs.Add();
                info.Range.Font.Size = 12; info.Range.Font.Bold = 0;
                info.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                info.Range.Text = $"Заказ №{order.id} от {order.orderDateTime:dd.MM.yyyy HH:mm}\n" +
                                  $"Клиент: {order.clients.surname} {order.clients.name}\n" +
                                  $"Статус: {order.orderStatuses.name}\n" +
                                  $"Оплата: {order.paymentMethods?.name}\n\n";

                var table = doc.Tables.Add(doc.Paragraphs.Last.Range, items.Count + 1, 4);
                table.Borders.Enable = 1;
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Товар";
                table.Cell(1, 3).Range.Text = "Кол-во";
                table.Cell(1, 4).Range.Text = "Сумма";

                for (int i = 0; i < items.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                    table.Cell(i + 2, 2).Range.Text = items[i].products.name;
                    table.Cell(i + 2, 3).Range.Text = items[i].productCount.ToString();
                    table.Cell(i + 2, 4).Range.Text = (items[i].productPrice * items[i].productCount).ToString("C");
                }

                var total = doc.Paragraphs.Add();
                total.Range.Font.Size = 14; total.Range.Font.Bold = 1;
                total.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                total.Range.Text = $"\nИТОГО: {order.summ:C}\n\n";

                var sign = doc.Paragraphs.Add();
                sign.Range.Font.Size = 10;
                sign.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                sign.Range.Text = "___________________ / ___________________";

                // Находим путь к стандартной системной папке "Мои документы"
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Объединяем путь: Документы + Наша новая папка
                string folderPath = Path.Combine(documentsPath, "Чеки HardwareStore");

                // Проверяем, существует ли папка. Если нет — создаем ее
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Формируем уникальное имя файла для чека (например: Чек_Заказ_5.docx)
                string fileName = $"Чек_Заказ_{order.id}.docx";
                string fullPath = Path.Combine(folderPath, fileName);

                // Сохраняем документ по указанному пути
                doc.SaveAs2(fullPath);
            }
        }
    }
}