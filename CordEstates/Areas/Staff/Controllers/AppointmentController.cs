﻿using AutoMapper;
using CordEstates.Areas.Staff.Models.DTOs;
using CordEstates.Entities;
using CordEstates.Helpers;
using CordEstates.Models;
using CordEstates.Wrappers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReflectionIT.Mvc.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CordEstates.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff, Admin")]
    public class AppointmentController : Controller
    {


        readonly IMapper _mapper;
        readonly IRepositoryWrapper _repositoryWrapper;

        readonly ILoggerManager _logger;



        public AppointmentController(ILoggerManager logger, IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;

        }

        // GET: Admin/Appointment
        public async Task<IActionResult> Index(string sortOrder,int pageNumber =1)
        {
            var data = _mapper.Map<List<AppointmentManagementDTO>>(await _repositoryWrapper.Appointment.GetAllAppointmentsAsync());
            IQueryable<AppointmentManagementDTO> sorted = data.AsQueryable();
            ViewData["UserNameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "username_desc" : "";
            ViewData["TimeSortParm"] = sortOrder == "Time" ? "time_desc" : "Time";
            ViewData["ListingSortParm"] = sortOrder == "Listing" ? "listing_desc" : "Listing";
            ViewData["currentSort"] = sortOrder;
            sorted = SortList(sortOrder, sorted);


            var model =  PaginatedList<AppointmentManagementDTO>.Create(sorted,pageNumber,5);
            
           
            return View(nameof(Index), model);
        }

        private static IQueryable<AppointmentManagementDTO> SortList(string sortOrder, IQueryable<AppointmentManagementDTO> sorted)
        {
            sorted = sortOrder switch
            {
                "Time" => sorted.OrderBy(t => t.Time).AsQueryable(),
                "time_desc" => sorted.OrderByDescending(t => t.Time).AsQueryable(),
                "listing_desc" => sorted.OrderByDescending(l => l.Listing.Address.FirstLine).AsQueryable(),
                "Listing" => sorted.OrderBy(l => l.Listing.Address.FirstLine).AsQueryable(),
                "username_desc" => sorted.OrderByDescending(s => s.Staff.UserName).AsQueryable(),
                _ => sorted.OrderBy(s => s.Staff.UserName).AsQueryable(),
            };
            return sorted;
        }

        // GET: Admin/Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarn($"Null value passed to method {nameof(Details)} in AppointmentController");
                return RedirectToAction(nameof(Index));
            }

            var appointment = _mapper.Map<AppointmentManagementDTO>(await _repositoryWrapper.Appointment.GetAppointmentByIdAsync(id));


            return View(nameof(Details), appointment);
        }


        public async Task<IActionResult> Create()
        {
            List<SelectListItem> list = await ParseListingForSelectList();
            CreateAppointmentDTO app = new CreateAppointmentDTO()
            { StaffId = await _repositoryWrapper.Employee.GetUserId(User) };

            ViewBag.listing = list;
            ViewData["UserId"] = await _repositoryWrapper.Employee.GetUserId(User);
            return View(nameof(Create), app);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentDTO appointment)
        {
            if (ModelState.IsValid)
            {
                Appointment app = _mapper.Map<Appointment>(appointment);


                _repositoryWrapper.Appointment.CreateAppointment(app);
                await _repositoryWrapper.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nameof(Create), appointment);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarn($"Invalid value passed to method {nameof(Edit)} in AppointmentController");
                return RedirectToAction(nameof(Index));
            }


            EditAppointmentManagementDTO appointment = _mapper.Map<EditAppointmentManagementDTO>(await _repositoryWrapper.Appointment.GetAppointmentByIdAsync(id));



            List<Listing> x = await _repositoryWrapper.Listing.GetAllListingsAsync();
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (var item in x)
            {
                if (id == item.Id)
                {
                    list.Add(new SelectListItem($"{item}", $"{item.Id}", true));
                }
                else
                {
                    list.Add(new SelectListItem($"{item}", $"{item.Id}"));
                }
            }
            ViewBag.listing = list;

            return View(nameof(Edit), appointment);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAppointmentManagementDTO appointment)
        {
            if (id != appointment.Id)
            {
                _logger.LogWarn($" Id: {id} does not match appointmentId value ");
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                 
                    Appointment app = _mapper.Map<Appointment>(appointment);


                    _repositoryWrapper.Appointment.UpdateAppointment(app);
                    await _repositoryWrapper.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"error occurred when updating {appointment.Id}. {ex}");

                }
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Edit), appointment);
        }



        private async Task<List<SelectListItem>> ParseListingForSelectList()
        {
            List<Listing> x = await _repositoryWrapper.Listing.GetAllListingsAsync();
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (var item in x)
            {
                list.Add(new SelectListItem($"{item}", $"{item.Id}"));
            }

            return list;
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarn($"Null value passed to method {nameof(Details)} in AppointmentController");
                return RedirectToAction(nameof(Index));
            }

            var appointment = _mapper.Map<AppointmentManagementDTO>(await _repositoryWrapper.Appointment.GetAppointmentByIdAsync(id));


            return View(nameof(Delete), appointment);
        }

        // POST: Admin/Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Appointment appointment = await _repositoryWrapper.Appointment.GetAppointmentByIdAsync(id);

                _repositoryWrapper.Appointment.DeleteAppointment(appointment);
                await _repositoryWrapper.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"unable to delete Appointment: {ex}");
                return RedirectToAction(nameof(Delete), id);
            }
        }
    }


}

