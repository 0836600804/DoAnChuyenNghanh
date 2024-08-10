using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyBanSach.Models;
using System.IO;
using System.Data.Linq.SqlClient;
using QuanLyBanSach.Attributes;

namespace QuanLyBanSach.Controllers
{
    public class AdminController : Controller
    {
        QLBanSachDataContext db = new QLBanSachDataContext();
        //---------------------Sách---------------------------------
        [CustomAuthorize(RolesName = "Admin")]
        public ActionResult AdminIndex()
        {
            ViewBag.Title = "Trang Admin";
            return View();
        }
        public ActionResult List_Sach()//Hiển thị cho trang admin
        {
            var lists = db.Saches.ToList();
            return View(lists);
        }
        public ActionResult Detail_Sach(int id = 0)
        {
            Sach book = db.Saches.Single(x => x.MaSach == id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }
        public ActionResult Delete_Sach(int id)
        {
            // Tìm tất cả các chi tiết tác giả liên quan đến sách
            var donhang = db.ChiTietDonHangs.Where(s => s.MaSach == id).ToList();
            var authorDetails = db.ChiTietTacGias.Where(s => s.MaSach == id).ToList();  // Xóa các tác giả có khỏa ngoại liên quan tới sách đã chọn
            var bookToDelete = db.Saches.FirstOrDefault(s => s.MaSach == id); // Xóa 1 sách được chọn 
            if (bookToDelete != null)
            {
                foreach(var dh in donhang)
                {
                    db.ChiTietDonHangs.DeleteOnSubmit(dh);
                }
                // Xóa tất cả các chi tiết tác giả liên quan
                foreach (var x in authorDetails) // chon x(bien hinh thuc) trong list tac gia
                {
                    db.ChiTietTacGias.DeleteOnSubmit(x);
                }
                // Xóa sách
                db.Saches.DeleteOnSubmit(bookToDelete);//Linq - DeleteOnSubmit EF-Remove
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges(); // SaveChange();
                return RedirectToAction("List_Sach"); // Chuyển hướng đến action SachList sau khi xóa thành công
            }
            else
            {
                return HttpNotFound(); // Trả về lỗi 404 Not Found nếu không tìm thấy sách để xóa
            }
        }
        public ActionResult Create_Sach()
        {
            ViewBag.DanhMucList = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            ViewBag.NhaXuatBanList = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Sach([Bind(Include = "MaSach, MaDM, MaNXB, TenSach, DoiTuong, Kho, SoTrang, TrongLuong, DinhDang, GiaBan, MoTa, NgayCapNhat, AnhBia, SLTon")] Sach sach, HttpPostedFileBase AnhBiaFile)
        {
            if (ModelState.IsValid)
            {
                // Lưu ảnh bìa vào thư mục trong project
                if (AnhBiaFile != null && AnhBiaFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(AnhBiaFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/sach/"), fileName);
                    AnhBiaFile.SaveAs(path);
                    sach.AnhBia = fileName; // Lưu tên ảnh vào trường AnhBia trong database
                }

                sach.NgayCapNhat = DateTime.Now; // Cập nhật thời gian hiện tại cho NgayCapNhat
                db.Saches.InsertOnSubmit(sach);
                db.SubmitChanges();
                return RedirectToAction("List_Sach");
            }

            // Nếu ModelState không hợp lệ, ta cần set lại SelectList cho dropdownlist ở view
            ViewBag.DanhMucList = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            ViewBag.NhaXuatBanList = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB");
            return View(sach);
        }
        public ActionResult Edit_Sach(int id)
        {
            ViewBag.DanhMucList = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            ViewBag.NhaXuatBanList = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB");
            var sach = db.Saches.SingleOrDefault(p => p.MaSach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            return View(sach);
        }

        // POST: TacGia/Edit_TacGia/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Sach([Bind(Include = "MaSach, MaDM, MaNXB, TenSach, DoiTuong, Kho, SoTrang, TrongLuong, DinhDang, GiaBan, MoTa, NgayCapNhat, AnhBia, SLTon")] Sach sach, HttpPostedFileBase AnhBiaFile)
        {
            if (ModelState.IsValid)
            {
                var sachInDb = db.Saches.SingleOrDefault(p => p.MaSach == sach.MaSach);
                if (sachInDb != null)
                {
                    sachInDb.MaDM = sach.MaDM;
                    sachInDb.MaNXB = sach.MaNXB;
                    sachInDb.TenSach = sach.TenSach;
                    sachInDb.DoiTuong = sach.DoiTuong;
                    sachInDb.Kho = sach.Kho;
                    sachInDb.SoTrang = sach.SoTrang;
                    sachInDb.TrongLuong = sach.TrongLuong;
                    sachInDb.DinhDang = sach.DinhDang;
                    sachInDb.GiaBan = sach.GiaBan;
                    sachInDb.MoTa = sach.MoTa;
                    sachInDb.SLTon = sach.SLTon;
                    if (AnhBiaFile != null && AnhBiaFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(AnhBiaFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/sach/"), fileName);
                        AnhBiaFile.SaveAs(path);
                        sachInDb.AnhBia = fileName; // Lưu tên ảnh vào trường AnhBia trong database
                    }

                    sachInDb.NgayCapNhat = DateTime.Now; // Cập nhật thời gian hiện tại cho NgayCapNhat
                    db.SubmitChanges();
                    return RedirectToAction("List_Sach");
                }
            }
            ViewBag.DanhMucList = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            ViewBag.NhaXuatBanList = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB");
            return View(sach);
        }
        //---------------------------Tác giả-----------------------------------------
        public ActionResult List_TacGia()
        {
            var listt = db.TacGias.ToList();
            return View(listt);
        }
        public ActionResult Detail_TacGia(int id)
        {
            var tacgia = db.TacGias.SingleOrDefault(p => p.MaTG == id);
            if (tacgia == null)
            {
                return HttpNotFound();
            }
            return View(tacgia);
        }
        public ActionResult Create_TacGia()
        {
            return View();
        }
        // POST: TacGia/Create_TacGia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_TacGia([Bind(Include = "MaTG, TenTG, DienThoai, TieuSu, DiaChi")] TacGia tacgia)
        {
            if (ModelState.IsValid)
            {
                db.TacGias.InsertOnSubmit(tacgia);
                db.SubmitChanges();
                return RedirectToAction("List_TacGia");
            }

            return View(tacgia);
        }

        // GET: TacGia/Edit_TacGia/5
        public ActionResult Edit_TacGia(int id)
        {
            var tacgia = db.TacGias.SingleOrDefault(p => p.MaTG == id);
            if (tacgia == null)
            {
                return HttpNotFound();
            }
            return View(tacgia);
        }

        // POST: TacGia/Edit_TacGia/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_TacGia([Bind(Include = "MaTG, TenTG, DienThoai, TieuSu, DiaChi")] TacGia tacgia)
        {
            if (ModelState.IsValid)
            {
                var tacgiaInDb = db.TacGias.SingleOrDefault(p => p.MaTG == tacgia.MaTG);
                if (tacgiaInDb != null)
                {
                    tacgiaInDb.TenTG = tacgia.TenTG;
                    tacgiaInDb.DienThoai = tacgia.DienThoai;
                    tacgiaInDb.TieuSu = tacgia.TieuSu;
                    tacgiaInDb.DiaChi = tacgia.DiaChi;
                    db.SubmitChanges();
                    return RedirectToAction("List_TacGia");
                }
            }
            return View(tacgia);
        }

        // GET: TacGia/Delete_TacGia/5
        public ActionResult Delete_TacGia(int id)
        {
            // Tìm tất cả các chi tiết sách liên quan đến tác giả
            var bookDetails = db.ChiTietTacGias.Where(ct => ct.MaTG == id).ToList();
            var authorToDelete = db.TacGias.FirstOrDefault(tg => tg.MaTG == id);
            if (authorToDelete != null)
            {
                // Xóa tất cả các chi tiết sách liên quan
                foreach (var bookDetail in bookDetails)
                {
                    db.ChiTietTacGias.DeleteOnSubmit(bookDetail);
                }
                // Xóa tác giả
                db.TacGias.DeleteOnSubmit(authorToDelete);
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();
                return RedirectToAction("List_TacGia"); // Chuyển hướng đến action List_TacGia sau khi xóa thành công
            }
            else
            {
                return HttpNotFound(); // Trả về lỗi 404 Not Found nếu không tìm thấy tác giả để xóa
            }
        }
        //---------------------------Nhà xuất bản --------------------------------------
        public ActionResult List_NXB()
        {
            var listt = db.NhaXuatBans.ToList();
            return View(listt);
        }
        public ActionResult Detail_NXB(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            NhaXuatBan nhaXuatBan = db.NhaXuatBans.SingleOrDefault(p => p.MaNXB == id);
            if (nhaXuatBan == null)
            {
                return HttpNotFound();
            }

            return View(nhaXuatBan);
        }
        public ActionResult Create_NXB()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_NXB([Bind(Include = "MaNXB, TenNXB, DiaChi, DienThoai")] NhaXuatBan nhaXuatBan)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem MaNXB có đúng độ dài là 5 ký tự không
                if (nhaXuatBan.MaNXB.Length != 5)
                {
                    ModelState.AddModelError("MaNXB", "Mã NXB phải có độ dài là 5 ký tự.");
                    return View(nhaXuatBan);
                }

                db.NhaXuatBans.InsertOnSubmit(nhaXuatBan);
                db.SubmitChanges();
                return RedirectToAction("List_NXB");
            }

            return View(nhaXuatBan);
        }

        public ActionResult Edit_XNB(string id)
        {
            NhaXuatBan nhaXuatBan = db.NhaXuatBans.SingleOrDefault(p => p.MaNXB == id);
            if (nhaXuatBan == null)
            {
                return HttpNotFound();
            }

            return View(nhaXuatBan);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_NXB([Bind(Include = "MaNXB, TenNXB, DiaChi, DienThoai")] NhaXuatBan nhaXuatBan)
        {
            if (ModelState.IsValid)
            {
                var nhaXuatBanInDb = db.NhaXuatBans.SingleOrDefault(p => p.MaNXB == nhaXuatBan.MaNXB);
                if (nhaXuatBanInDb != null)
                {
                    nhaXuatBanInDb.TenNXB = nhaXuatBan.TenNXB;
                    nhaXuatBanInDb.DiaChi = nhaXuatBan.DiaChi;
                    nhaXuatBanInDb.DienThoai = nhaXuatBan.DienThoai;
                    db.SubmitChanges();
                    return RedirectToAction("List_NXB");
                }
            }

            return View(nhaXuatBan);
        }
        public ActionResult Delete_NXB(string id)
        {
            // Tìm tất cả các sách liên quan đến nhà xuất bản
            var relatedBooks = db.Saches.Where(s => s.MaNXB == id).ToList();
            var publisherToDelete = db.NhaXuatBans.FirstOrDefault(nxb => nxb.MaNXB == id);
            if (publisherToDelete != null)
            {
                // Xóa tất cả các sách liên quan
                foreach (var book in relatedBooks)
                {
                    db.Saches.DeleteOnSubmit(book);
                }
                // Xóa nhà xuất bản
                db.NhaXuatBans.DeleteOnSubmit(publisherToDelete);
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();
                return RedirectToAction("List_NXB"); // Chuyển hướng đến action List_NXB sau khi xóa thành công
            }
            else
            {
                return HttpNotFound(); // Trả về lỗi 404 Not Found nếu không tìm thấy nhà xuất bản để xóa
            }
        }

        //--------------------------Danh mục----------------------------
        public ActionResult List_DM()
        {
            var listt = db.DanhMucs.ToList();
            return View(listt);
        }
        public ActionResult Detail_DM(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DanhMuc danhMuc = db.DanhMucs.SingleOrDefault(p => p.MaDM == id);
            if (danhMuc == null)
            {
                return HttpNotFound();
            }

            return View(danhMuc);
        }
        public ActionResult Create_DM()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_DM([Bind(Include = "MaDM, TenDM")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem MaNXB có đúng độ dài là 5 ký tự không
                if (danhMuc.MaDM.Length != 4)
                {
                    ModelState.AddModelError("MaDM", "Mã Danh mục phải có độ dài là 4 ký tự.");
                    return View(danhMuc);
                }

                db.DanhMucs.InsertOnSubmit(danhMuc);
                db.SubmitChanges();
                return RedirectToAction("List_DM");
            }

            return View(danhMuc);
        }
        public ActionResult Edit_DM(string id)
        {
            DanhMuc danhMuc = db.DanhMucs.SingleOrDefault(p => p.MaDM == id);
            if (danhMuc == null)
            {
                return HttpNotFound();
            }

            return View(danhMuc);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_DM([Bind(Include = "MaDM, TenDM")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                var danhMucInDb = db.DanhMucs.SingleOrDefault(p => p.MaDM == danhMuc.MaDM);
                if (danhMucInDb != null)
                {
                    danhMucInDb.TenDM = danhMuc.TenDM;
                    db.SubmitChanges();
                    return RedirectToAction("List_DM");
                }
            }

            return View(danhMuc);
        }
        public ActionResult Delete_DM(string id)
        {
            // Tìm tất cả các sách liên quan đến nhà xuất bản
            var relatedBooks = db.Saches.Where(s => s.MaDM == id).ToList();
            var publisherToDelete = db.DanhMucs.FirstOrDefault(dm => dm.MaDM == id);
            if (publisherToDelete != null)
            {
                // Xóa tất cả các sách liên quan
                foreach (var book in relatedBooks)
                {
                    db.Saches.DeleteOnSubmit(book);
                }
                // Xóa nhà xuất bản
                db.DanhMucs.DeleteOnSubmit(publisherToDelete);
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();
                return RedirectToAction("List_DM"); // Chuyển hướng đến action List_NXB sau khi xóa thành công
            }
            else
            {
                return HttpNotFound(); // Trả về lỗi 404 Not Found nếu không tìm thấy nhà xuất bản để xóa
            }
        }
        //--------------------------Khách hàng----------------------------
        [Authorize]
        public ActionResult List_KH()
        {
            var listk = db.KhachHangs.ToList();
            return View(listk);
        }
        public ActionResult Detail_KH(int id)
        {
            KhachHang khachHang = db.KhachHangs.FirstOrDefault(k => k.MaKH == id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        public ActionResult Delete_KH(int id)
        {
            var KhachHang = db.KhachHangs.FirstOrDefault(s => s.MaKH == id);
            db.KhachHangs.DeleteOnSubmit(KhachHang);
            db.SubmitChanges();
            return RedirectToAction("List_KH");
        }
        //--------------------------AD----------------------------
        public ActionResult List_AD()
        {
            var lista = db.AdminSaches.ToList();
            return View(lista);
        }
        public ActionResult Detail_AD(int id)
        {
            AdminSach adminSach = db.AdminSaches.FirstOrDefault(a => a.MaAD == id);
            if (adminSach == null)
            {
                return HttpNotFound();
            }
            return View(adminSach);
        }

        public ActionResult Delete_AD(int id)
        {
            var Admin = db.AdminSaches.FirstOrDefault(s => s.MaAD == id);
            db.AdminSaches.DeleteOnSubmit(Admin);
            db.SubmitChanges();
            return RedirectToAction("List_AD");
        }
        public ActionResult Create_AD()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_AD([Bind(Include = "MaAD, TenAD, TaiKhoan, MatKhau")] AdminSach adminSach)
        {
            if(ModelState.IsValid)
            {
                db.AdminSaches.InsertOnSubmit(adminSach);
                db.SubmitChanges();
                return RedirectToAction("List_AD");
            }

            return View(adminSach);
        }
        public ActionResult Edit_AD(int id)
        {
            AdminSach adminSach = db.AdminSaches.SingleOrDefault(p => p.MaAD == id);
            if (adminSach == null)
            {
                return HttpNotFound();
            }

            return View(adminSach);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_AD([Bind(Include = "MaAD, TenAD, TaiKhoan, MatKhau")] AdminSach adminSach)
        {
            if (ModelState.IsValid)
            {
                var adminSachInDb = db.AdminSaches.SingleOrDefault(p => p.MaAD== adminSach.MaAD);
                if (adminSachInDb != null)
                {
                    adminSachInDb.TenAD = adminSach.TenAD;
                    adminSachInDb.TaiKhoan = adminSach.TaiKhoan;
                    adminSachInDb.MatKhau = adminSach.MatKhau;
                    db.SubmitChanges();
                    return RedirectToAction("List_AD");
                }
            }

            return View(adminSach);
        }
    }
}