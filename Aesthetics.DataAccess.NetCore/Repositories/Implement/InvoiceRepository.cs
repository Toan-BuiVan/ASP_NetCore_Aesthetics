using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseInvoice_Loggin;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class InvoiceRepository : BaseApplicationService, IInvoiceRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private IUserRepository _userRepository;
		private IVouchersRepository _vouchersRepository;
		private IProductsRepository _productsRepository;
		private IServicessRepository _servicessRepository;
		public InvoiceRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider, IUserRepository userRepository, 
			IVouchersRepository vouchersRepository, 
			IProductsRepository productsRepository, 
			IServicessRepository servicessRepository) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_userRepository = userRepository;
			_vouchersRepository = vouchersRepository;
			_productsRepository = productsRepository;
			_servicessRepository = servicessRepository;
		}

		public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		{
			//Tổng giá trị đơn hàng trước khi áp dụng voucher

			decimal totalMoney = 0;
			var returnData = new ResponseInvoice_Loggin();
			var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
			var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				Users empployee = null;
				Vouchers vouchers = null;
				var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);

				//1. Kiểm tra employee
				if (insert_.EmployeeID != null)
				{
					empployee = await _userRepository.GetUserByUserID(insert_.EmployeeID);
					if (empployee == null || empployee.TypePerson != "Employee")
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID không tồn tại!";
						return returnData;
					}
				}

				//2. Kiểm tra Customer
				if (customer == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "CustomerID không tồn tại!";
					return returnData;
				}

				//3. Kiểm tra danh sách đầu vào của Product & Services
				if (insert_.ProductIDs == null && insert_.ServicesIDs == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product || Servicess!";
					return returnData;
				}

				//4. Tính tổng giá trị đơn hàng
				if (insert_.ProductIDs != null)
				{
					for (int i = 0; i < insert_.ProductIDs.Count; i++)
					{
						var product = await _productsRepository.GetProductsByProductID(insert_.ProductIDs[i]);
						if (product == null) continue;
						totalMoney += insert_.QuantityProduct[i] * product.SellingPrice ?? 0;
					}
				}
				if (insert_.ServicesIDs != null)
				{
					for (int i = 0; i < insert_.ServicesIDs.Count; i++)
					{
						var service = await _servicessRepository.GetServicessByServicesID(insert_.ServicesIDs[i]);
						if (service == null) continue;
						totalMoney += insert_.QuantityServices[i] * service.PriceService ?? 0;
					}
				}

				//5. Kiểm tra VouchersID
				if (insert_.VoucherID != null)
				{
					vouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0);
					var wallets = await _context.Wallets.Where(s => s.VoucherID == insert_.VoucherID && s.UserID == insert_.CustomerID).FirstOrDefaultAsync();
					if (wallets == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Bạn chưa sở hữu VouchersID: {insert_.VoucherID}!";
						return returnData;
					}
					if (vouchers == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "VoucherID không tồn tại!";
						return returnData;
					}
					if (vouchers.StartDate < DateTime.Now)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Voucher đã hết hạn!";
						return returnData;
					}
					if (totalMoney < vouchers.MinimumOrderValue)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Voucher chỉ áp dụng cho đơn hàng tối thiểu {vouchers.MinimumOrderValue}!";
						return returnData;
					}
				}

				//1. Tạo hóa đơn
				var discount = vouchers?.DiscountValue ?? 0; 
				var maxDiscountValue = vouchers?.MaxValue ?? 0;
				var totalMonyVoucher = totalMoney * ((decimal)discount / 100);
				var finalDiscount = totalMonyVoucher > maxDiscountValue ? maxDiscountValue : totalMonyVoucher;
				var newInvoice = new Invoice
				{
					EmployeeID = empployee?.UserID,
					CustomerID = customer.UserID,
					VoucherID = vouchers?.VoucherID,
					Code = vouchers?.Code,
					DiscountValue = vouchers?.DiscountValue,
					TotalMoney = totalMoney - finalDiscount,
					DateCreated = DateTime.Now,
					Status = "Pending",
					DeleteStatus = 1,
					Type = "Ouput"
				};
				await _context.Invoice.AddAsync(newInvoice);
				await _context.SaveChangesAsync();
				invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
				{
					InvoiceID = newInvoice.InvoiceID,
					EmployeeID = newInvoice.EmployeeID,
					CustomerID = newInvoice.CustomerID,
					VoucherID = newInvoice.VoucherID,
					Code = vouchers?.Code,
					DiscountValue = vouchers?.DiscountValue,
					TotalMoney = newInvoice.TotalMoney,
					DateCreated = newInvoice.DateCreated,
					Status = newInvoice.Status,
					DeleteStatus = newInvoice.DeleteStatus,
					Type = newInvoice.Type,
				});

				//2. Tạo chi tiết hóa đơn
				//2.1 Tạo chi tiết hóa đơn khi có Product & Servicess
				if (insert_.ProductIDs != null && insert_.ServicesIDs != null && insert_.ProductIDs.Count > 0 && insert_.ServicesIDs.Count > 0)
				{
					int minCount = Math.Min(insert_.ProductIDs.Count, insert_.ServicesIDs.Count);
					for (int i = 0; i < minCount; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantityProduct = insert_.QuantityProduct[i];
						var product = await _productsRepository.GetProductsByProductID(productID);

						var servicesID = insert_.ServicesIDs[i];
						var quantityService = insert_.QuantityServices[i];
						var service = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						if (quantityProduct <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						if (service == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						if (quantityService <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							ServiceID = service.ServiceID,
							ServiceName = service.ServiceName,
							VoucherID = vouchers?.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							PriceService = service.PriceService,
							TotalQuantityProduct = quantityProduct,
							TotalQuantityService = quantityService,
							TotalMoney = (quantityProduct * product.SellingPrice)
										+ (quantityService * service.PriceService),
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}

					// Nếu có sản phẩm dư, tạo bản ghi riêng
					for (int i = minCount; i < insert_.ProductIDs.Count; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantityProduct = insert_.QuantityProduct[i];
						var product = await _productsRepository.GetProductsByProductID(productID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						if (quantityProduct <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
							return returnData;
						}
						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							TotalQuantityProduct = quantityProduct,
							TotalMoney = quantityProduct * product.SellingPrice,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}

					// Nếu có dịch vụ dư, tạo bản ghi riêng
					for (int i = minCount; i < insert_.ServicesIDs.Count; i++)
					{
						var servicesID = insert_.ServicesIDs[i];
						var quantityService = insert_.QuantityServices[i];
						var service = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (service == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}
						if (quantityService <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ServiceID = service.ServiceID,
							ServiceName = service.ServiceName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceService = service.PriceService,
							TotalQuantityService = quantityService,
							TotalMoney = quantityService * service.PriceService,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};
						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();
						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}

				//2.2 Tạo chi tiết hóa đơn khi có Product không có Services
				if (insert_.ProductIDs != null && insert_.ProductIDs.Count >= 1 && (insert_.ServicesIDs == null || insert_.ServicesIDs.Count == 0))
				{
					int productCount = insert_.ProductIDs.Count;

					for (int i = 0; i < productCount; i++)
					{
						var productID = insert_.ProductIDs[i];
						var quantity = insert_.QuantityProduct[i];

						var product = await _productsRepository.GetProductsByProductID(productID);
						if (product == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ProductID: {productID} không tồn tại!";
							return returnData;
						}
						if (quantity <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Vui lòng nhập lại số lượng của ProductID: {productID}!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ProductID = product.ProductID,
							ProductName = product.ProductName,
							VoucherID = newInvoice.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceProduct = product.SellingPrice,
							TotalQuantityProduct = quantity,
							TotalMoney = quantity * product.SellingPrice,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};

						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();

						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ProductID = newInvoiceDetail.ProductID,
							ProductName = newInvoiceDetail.ProductName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceProduct = newInvoiceDetail.PriceProduct,
							TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}
				
				// 2.3 Tạo chi tiết hóa đơn khi có Services không có Product
				if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0 && (insert_.ProductIDs == null || insert_.ProductIDs.Count == 0))
				{
					if (insert_.QuantityServices == null || insert_.ServicesIDs.Count != insert_.QuantityServices.Count)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Số lượng dịch vụ không khớp với danh sách dịch vụ.";
						return returnData;
					}

					for (int i = 0; i < insert_.ServicesIDs.Count; i++)
					{
						var servicesID = insert_.ServicesIDs[i];
						var quantity = insert_.QuantityServices[i];

						var servicess = await _servicessRepository.GetServicessByServicesID(servicesID);
						if (servicess == null)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"ServiceID: {servicesID} không tồn tại!";
							return returnData;
						}

						if (quantity <= 0)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = $"Vui lòng nhập lại số lượng của ServiceID: {servicesID}!";
							return returnData;
						}

						var newInvoiceDetail = new InvoiceDetail
						{
							InvoiceID = newInvoice.InvoiceID,
							CustomerID = newInvoice.CustomerID,
							CustomerName = customer.UserName,
							EmployeeID = newInvoice.EmployeeID,
							EmployeeName = empployee?.UserName,
							ServiceID = servicess.ServiceID,
							ServiceName = servicess.ServiceName,
							VoucherID = insert_.VoucherID,
							Code = newInvoice.Code,
							DiscountValue = newInvoice.DiscountValue,
							PriceService = servicess.PriceService,
							TotalQuantityService = quantity,
							TotalMoney = quantity * servicess.PriceService,
							DeleteStatus = 1,
							Status = newInvoice.Status,
							Type = newInvoice.Type,
						};

						await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
						await _context.SaveChangesAsync();

						invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
						{
							InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
							CustomerID = newInvoiceDetail.CustomerID,
							CustomerName = newInvoiceDetail.CustomerName,
							EmployeeID = newInvoiceDetail.EmployeeID,
							EmployeeName = newInvoiceDetail.EmployeeName,
							ServiceID = newInvoiceDetail.ServiceID,
							ServiceName = newInvoiceDetail.ServiceName,
							VoucherID = newInvoiceDetail.VoucherID ?? 0,
							Code = newInvoiceDetail.Code,
							DiscountValue = newInvoiceDetail.DiscountValue,
							PriceService = newInvoiceDetail.PriceService,
							TotalQuantityService = newInvoiceDetail.TotalQuantityService,
							TotalMoney = newInvoiceDetail.TotalMoney,
							DeleteStatus = newInvoiceDetail.DeleteStatus,
							Status = newInvoiceDetail.Status,
							Type = newInvoiceDetail.Type,
						});
					}
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = $"Insert_Invoice thành công!";
				returnData.InvoiceID = newInvoice.InvoiceID;
				returnData.TotalMoney = totalMoney;
				returnData.invoiceOut_Loggin = invoiceOut_Loggin;
				returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
				return returnData;

			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error in Insert_Invoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseInvoice_Loggin> Delete_Invoice(Delete_Invoice delete_)
		{
			var returnData = new ResponseInvoice_Loggin();
			var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
			var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(s => s.InvoiceID == delete_.InvoiceID && s.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					//1.Xóa Ivoice
					invoice.DeleteStatus = 0;
					invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
					{
						InvoiceID = invoice.InvoiceID,
						EmployeeID = invoice.EmployeeID,
						CustomerID = invoice.CustomerID,
						VoucherID = invoice.VoucherID,
						Code = invoice?.Code,
						DiscountValue = invoice?.DiscountValue,
						DateCreated = invoice.DateCreated,
						Status = invoice.Status,
						DeleteStatus = invoice.DeleteStatus,
						Type = invoice.Type,
					});


					//2. Xóa InvoiceDetail
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1);
					if (invoiceDetail != null)
					{
						foreach(var item in invoiceDetail)
						{
							item.DeleteStatus = 0;
							invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
							{
								InvoiceDetailID = item.InvoiceDetailID,
								InvoiceID = item.InvoiceID,
								CustomerID = item.CustomerID,
								CustomerName = item.CustomerName,
								EmployeeID = item.EmployeeID,
								EmployeeName = item.EmployeeName,
								ProductID = item.ProductID,
								ProductName = item.ProductName,
								ServiceID = item.ServiceID,
								ServiceName = item.ServiceName,
								VoucherID = item.VoucherID ?? 0,
								Code = item.Code,
								DiscountValue = item.DiscountValue,
								PriceProduct = item.PriceProduct,
								PriceService = item.PriceService,
								TotalQuantityService = item.TotalQuantityService,
								TotalQuantityProduct = item.TotalQuantityProduct,
								TotalMoney = item.TotalMoney,
								DeleteStatus = item.DeleteStatus,
								Status = item.Status,
								Type = item.Type,
							});
						}
					}
					await transaction.CommitAsync();
					await _context.SaveChangesAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = $"Delete_Invoice thành công!";
					returnData.invoiceOut_Loggin = invoiceOut_Loggin;
					returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "InvoiceID không hợp lệ!";
				return returnData;
			}
			catch(Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error in Delete_Invoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseGetListInvoice> GetList_SearchInvoice(GetList_Invoice getList_)
		{
			var returnData = new ResponseGetListInvoice();
			try
			{
				if (getList_.InvoiceID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.InvoiceID == getList_.InvoiceID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.EmployeeID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.EmployeeID == getList_.EmployeeID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.EmployeeID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.CustomerID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.CustomerID == getList_.CustomerID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.CustomerID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.InvoiceType != null)
				{
					if (!Validation.CheckString(getList_.InvoiceType) || !Validation.CheckXSSInput(getList_.InvoiceType))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceType không hợp lệ!";
						return returnData;
					}
					if (!string.Equals(getList_.InvoiceType, "Input", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(getList_.InvoiceType, "Ouput", StringComparison.OrdinalIgnoreCase))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType chỉ chấp nhận Input || Ouput";
						return returnData;
					}
				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.EndDate < getList_.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@InvoiceID", getList_.InvoiceID ?? null);
				parameters.Add("@EmployeeID", getList_.EmployeeID ?? null);
				parameters.Add("@CustomerID", getList_.CustomerID ?? null);
				parameters.Add("@InvoiceType", getList_.InvoiceType ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				var _listInvoice = await DbConnection.QueryAsync<GetList_Invoice_Out>("GetList_SearchInvoice", parameters);
				if (_listInvoice != null && _listInvoice.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Invoice thành công!";
					returnData.Data = _listInvoice.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Invoice nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseGetListInvoiceDetail> GetList_SearchInvoiceDetail(GetList_InvoiceDetail getList_)
		{
			var returnData = new ResponseGetListInvoiceDetail();
			try
			{
				if (getList_.InvoiceID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.InvoiceID == getList_.InvoiceID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.InvoiceDetailType != null)
				{
					if (!Validation.CheckString(getList_.InvoiceDetailType) || !Validation.CheckXSSInput(getList_.InvoiceDetailType))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType không hợp lệ!";
						return returnData;
					}
					if (!string.Equals(getList_.InvoiceDetailType, "Input", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(getList_.InvoiceDetailType, "Ouput", StringComparison.OrdinalIgnoreCase))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType chỉ chấp nhận Input || Ouput";
						return returnData;
					}

				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.EndDate < getList_.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@InvoiceID", getList_.InvoiceID ?? null);
				parameters.Add("@InvoiceDetailType", getList_.InvoiceDetailType ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				var _listInvoice = await DbConnection.QueryAsync<GetList_InvoiceDetail_Out>("GetList_SearchInvoiceDetail", parameters);
				if (_listInvoice != null && _listInvoice.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Invoice thành công!";
					returnData.Data = _listInvoice.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Invoice nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchInvoiceDetail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Invoice> GetInvoiceByInvoiceID(int InvoiceID)
		{
			return await _context.Invoice.Where(s => s.InvoiceID == InvoiceID && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task UpdateStatusInvoice(int InvoiceID)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(a => a.InvoiceID == InvoiceID && a.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.Status = "Paid";
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
					if (invoiceDetail != null)
					{
						foreach(var item in invoiceDetail)
						{
							item.Status = "Paid";
						}
					}
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				else
				{
					throw new Exception($"Không tồn tại InvoiceID: {InvoiceID}");
				}

			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error UpdateStatusInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<List<InvoiceDetail>> InvoiceDetailByInvoiceID(int invoiceID)
		{
			return await _context.InvoiceDetail.Where(s => s.InvoiceID == invoiceID && s.DeleteStatus == 1).ToListAsync();
		}


		public async Task UpdateStatusInvoiceDetail(int InvoiceID)
		{
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(v => v.InvoiceID == InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice != null)
				{
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1)
						.ToList();
					if (invoiceDetail != null && invoiceDetail.Any())
					{
						foreach (var detail in invoiceDetail)
						{
							detail.Status = "Paid";
						}
						await _context.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in UpdateStatusInvoiceDetail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
