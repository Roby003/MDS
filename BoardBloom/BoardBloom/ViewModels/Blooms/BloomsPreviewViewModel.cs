using BoardBloom.Data;
using BoardBloom.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BoardBloom.ViewModels {
    public class BloomsPreviewViewModel {
        public Bloom Bloom { get; set; }

        public int BoardId { get; set; }
    }
}